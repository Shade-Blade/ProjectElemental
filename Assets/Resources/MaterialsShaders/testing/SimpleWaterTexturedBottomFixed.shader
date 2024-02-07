Shader "Custom/SimpleWater"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
        _TextureScale("Scale", float) = 1
        _TextureLength("Texture Length", float) = 1
        _TextureAlpha("Texture Alpha", float) = 1
        _Color("Color", Color) = (0,0,0,0)
        _GlowColor("Bottom Color", Color) = (1, 1, 1, 1)
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _FoamLength("Foam Length", float) = 0.15
        _FoamAlpha("Foam Alpha", float) = 0.85
        //_SpecColor("Specular Color", Color) = (1, 1, 1, 1)
        //_SpecPower("Spec Strength", float) = 1
        _FadeLength("Fade Length", float) = 0.15
        _TopAlpha("Top Alpha", float) = 0.15
        _BottomAlpha("Bottom Alpha", float) = 0.15
        _HorizonColor("Horizon Color", Color) = (1, 1, 1, 1)
        _HorizonPower("Horizon Power", float) = 1
        _FixedDepth("Fixed Depth", float) = 1
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
				float3 viewDir : TEXCOORD1;
                float4 worldPos : POSITION2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float _TextureScale;

            float _TopAlpha;
            float _BottomAlpha;
            float _HorizonPower;
            float _FoamAlpha;
            float _TextureAlpha;
            //float _SpecPower;

            float _FixedDepth;

            v2f vert(appdata v, out float4 vertex : SV_POSITION)
            {
                v2f o;
                vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float4 normal4 = float4(v.normal, 0.0);
                o.normal = normalize(mul(normal4, unity_WorldToObject).xyz);
                o.viewDir = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            sampler2D _CameraDepthTexture;
            fixed4 _Color;
            fixed4 _GlowColor;
            fixed4 _FoamColor;
            //fixed4 _SpecColor;
            float _FadeLength;
            float _TextureLength;
            float _FoamLength;

            fixed4 _HorizonColor;
            #include "Lighting.cginc"

            half4 frag (v2f i, UNITY_VPOS_TYPE vpos : VPOS) : SV_Target
            {
                float2 screenuv = vpos.xy / _ScreenParams.xy;
                float screenDepth = LinearEyeDepth(tex2D(_CameraDepthTexture, screenuv));

                //float4 realPos = mul(unity_ObjectToWorld, i.vertex);

                float3 viewPlane = i.viewDir.xyz / dot(i.viewDir.xyz, unity_WorldToCamera._m20_m21_m22);

                // calculate the world position
                // multiply the view plane by the linear depth to get the camera relative world space position
                // add the world space camera position to get the world space position from the depth texture
                float3 worldPos = viewPlane * screenDepth + _WorldSpaceCameraPos;

                float diff = _FixedDepth - worldPos.y;

                float intersect = 0;

                intersect = diff / _FadeLength;

                intersect = saturate(intersect);
                fixed4 glowColor;

                float oldIntersect = 0;

                if (diff < _FoamLength) {
                    glowColor = _FoamColor;
                    glowColor.a = _FoamAlpha;
                } else {
                    oldIntersect = saturate(diff / _TextureLength);
                    if (oldIntersect == 1) {
                        //Walk back the world pos (Problem: ratio thing needs to work differently)

                        //

                        float diffRatio = (_TextureLength - _FixedDepth + i.worldPos.y) / (i.worldPos.y - worldPos.y);

                        if (diffRatio <= 0) {
                            worldPos = i.worldPos;
                        } else {
                            worldPos = i.worldPos + (worldPos - i.worldPos) * diffRatio;
                        }

                        float2 wpB = float2(worldPos.x, worldPos.z);
                        glowColor = tex2D(_MainTex, wpB * _TextureScale);
                    } else {
                        diff += length(i.worldPos - worldPos);
                        intersect = saturate(diff / _FadeLength);
                        glowColor = fixed4(lerp(_Color.rgb, _GlowColor, intersect), intersect);
                    }
                }

                intersect = (_BottomAlpha - _TopAlpha) * intersect + _TopAlpha;                

                half4 col = glowColor;

                float fdot = 1 - dot(i.normal, normalize(i.viewDir));
                fdot = pow(fdot, _HorizonPower);

                col = lerp(glowColor, _HorizonColor, fdot);

                // spec
                /*
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
                float spec = dot(normalize(normalize(i.viewDir.xyz) + lightDir), i.normal.xyz);// specular based on view and light direction
                float cutOff = step(saturate(spec), 0.8); // cutoff for where base color is
                if (length(_WorldSpaceLightPos0.xyz) == 0) {
                    cutOff = 1;
                }
                float3 specularMain = col.rgb * (1 - cutOff) * _SpecPower * _SpecColor * 4;// inverted base cutoff times specular color
                
                col.rgb += specularMain;
                */

                if (oldIntersect == 1) {
                    col.a = _TextureAlpha;
                } else {
                    col.a = intersect;
                }
                if (diff < _FoamLength) {
                    col.a = _FoamAlpha;
                }

                fixed4 lightPos = _WorldSpaceLightPos0;
                fixed3 lightDir = lightPos.xyz;    // Light direction

                if (lightPos.w == 1) {
                    lightDir = normalize(lightPos.xyz - i.worldPos);
                }

                //Problem: can only apply to one light at a time
                //Usually the sun light specular can't be seen
                half3 h = normalize (lightDir + normalize(i.viewDir));

                float nh = max (0, dot (i.normal, h));
                float spec = pow (nh, 24);

                col += half4(_LightColor0.rgb * spec, spec);

                col.a = clamp(col.a, 0, 1);

                return col;
            }
            ENDCG
        }
    }
}