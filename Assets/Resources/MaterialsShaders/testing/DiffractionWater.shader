Shader "Custom/DiffractionWater"
{
    Properties
    {
        _Color("Color", Color) = (0,0,0,0)
        _GlowColor("Glow Color", Color) = (1, 1, 1, 1)
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _FoamLength("Foam Length", float) = 0.15
        _FoamAlpha("Foam Alpha", float) = 0.85
        _FadeLength("Fade Length", float) = 0.15
        _TopAlpha("Top Alpha", float) = 0.15
        _BottomAlpha("Bottom Alpha", float) = 0.15
        _HorizonColor("Horizon Color", Color) = (1, 1, 1, 1)
        _HorizonPower("Horizon Power", float) = 1
        
        _Intensity ("Intensity", float) = 0
        _Omega ("Omega", float) = 0
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
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
                float4 grabPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TopAlpha;
            float _BottomAlpha;
            float _HorizonPower;
            float _FoamAlpha;

            v2f vert(appdata v, out float4 vertex : SV_POSITION)
            {
                v2f o;
                vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float4 normal4 = float4(v.normal, 0.0);
                o.normal = normalize(mul(normal4, unity_WorldToObject).xyz);
                o.viewDir = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            sampler2D _CameraDepthTexture;
            sampler2D _BackgroundTexture;
            fixed4 _Color;
            fixed4 _GlowColor;
            fixed4 _FoamColor;
            float _FadeLength;
            float _FoamLength;
            
            float _Intensity;
            float _Omega;

            fixed4 _HorizonColor;

            fixed4 frag (v2f i, UNITY_VPOS_TYPE vpos : VPOS) : SV_Target
            {
                float2 screenuv = vpos.xy / _ScreenParams.xy;
                float screenDepth = LinearEyeDepth(tex2D(_CameraDepthTexture, screenuv));

                //float4 realPos = mul(unity_ObjectToWorld, i.vertex);

                float3 viewPlane = i.viewDir.xyz / dot(i.viewDir.xyz, unity_WorldToCamera._m20_m21_m22);

                // calculate the world position
                // multiply the view plane by the linear depth to get the camera relative world space position
                // add the world space camera position to get the world space position from the depth texture
                float3 worldPos = viewPlane * screenDepth + _WorldSpaceCameraPos;

                float diff = i.worldPos.y - worldPos.y;

                float intersect = 0;


                intersect = diff / _FadeLength;

                intersect = saturate(intersect);
                fixed4 glowColor = fixed4(lerp(_Color.rgb, _GlowColor, intersect), intersect);

                if (diff < _FoamLength) {
                    glowColor = _FoamColor;
                    glowColor.a = _FoamAlpha;
                }

                intersect = (_BottomAlpha - _TopAlpha) * intersect + _TopAlpha;


                fixed4 col = glowColor;

                float fdot = 1 - dot(i.normal, normalize(i.viewDir));
                fdot = pow(fdot, _HorizonPower);

                col = lerp(glowColor, _HorizonColor, fdot);

                col.a = intersect;
                if (diff < _FoamLength) {
                    col.a = _FoamAlpha;
                }

                float camdist = screenDepth;    //probably wrong

                //float4 newgrabpos = i.grabPos;
                //newgrabpos.x += sin((_Time.y + newgrabpos.y) * _Intensity)/20;

                float4 checkPos = i.grabPos;
                checkPos.x += diff * (1 / camdist) * _Intensity * sin((_Time.y + checkPos.y) * _Omega)/20;

                fixed refrFix = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(checkPos)));

                half4 bgcolor;

                if (LinearEyeDepth(refrFix) < i.grabPos.w) {
                    bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos);
                }
                else {
                    bgcolor = tex2Dproj(_BackgroundTexture, checkPos);
                }

                bgcolor = lerp(bgcolor, col, col.a);

                bgcolor.a = 1;

                return bgcolor;
            }
            ENDCG
        }
    }
}