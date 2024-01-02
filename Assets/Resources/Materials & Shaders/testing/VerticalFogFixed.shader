Shader "Custom/VerticalFogFixed"
{
    Properties
    {
        _Color("Color", Color) = (0,0,0,0)
        _GlowColor("Bottom Color", Color) = (1, 1, 1, 1)
        _FadeLength("Fade Length", float) = 0.15
        _TopAlpha("Top Alpha", float) = 0.15
        _BottomAlpha("Bottom Alpha", float) = 0.15
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

            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            float _TopAlpha;
            float _BottomAlpha;
            //float _SpecPower;

            float _FixedDepth;

            v2f vert(appdata v, out float4 vertex : SV_POSITION)
            {
                v2f o;
                vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float4 normal4 = float4(v.normal, 0.0);
                o.normal = normalize(mul(normal4, unity_WorldToObject).xyz);
                o.viewDir = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            sampler2D _CameraDepthTexture;
            fixed4 _Color;
            fixed4 _GlowColor;
            //fixed4 _SpecColor;
            float _FadeLength;

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

                //accounts for light having to travel through the material to reach your eyes (but doesn't really look right for this type of fog?)
                //diff += length(i.worldPos - worldPos);
                
                //exponential fog version
                //intersect = 1 - exp(-diff / _FadeLength);

                intersect = saturate(diff / _FadeLength);
                glowColor = fixed4(lerp(_Color.rgb, _GlowColor, intersect), intersect);

                intersect = (_BottomAlpha - _TopAlpha) * intersect + _TopAlpha;


                half4 col = glowColor;

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

                col.a = intersect;

                col.a = clamp(col.a, 0, 1);

                return col;
            }
            ENDCG
        }
    }
}