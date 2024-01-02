Shader "Custom/Shield"
{
    Properties
    {        
		_MainTex("Texture", 2D) = "white" {}
        [HDR]
        _Color("Color", Color) = (0,0,0,0)
        [HDR]
        _GlowColor("Glow Color", Color) = (1, 1, 1, 1)
        _FadeLength("Fade Length", Range(0, 2)) = 0.15
        _EdgeLength("Edge Length", float) = 0.15
        _EdgeExponent("Edge Exponent", float) = 0.15
        _EdgeThreshold("Edge Threshold", float) = 0.15
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
                
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float3 viewDir : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.pos = UnityObjectToClipPos(v.vertex);

                float4 normal4 = float4(v.normal, 0.0);
				o.normal = normalize(mul(normal4, unity_WorldToObject).xyz);
                o.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);

                return o;
            }

            sampler2D _CameraDepthTexture;
            half4 _Color;
            half4 _GlowColor;
            float _FadeLength;
            float _EdgeLength;
            float _EdgeExponent;
            float _EdgeThreshold;

            half4 frag (v2f i) : SV_Target
            {
                float2 screenuv = i.pos.xy / _ScreenParams.xy;
                float screenDepth = Linear01Depth(tex2D(_CameraDepthTexture, screenuv));
                float diff = screenDepth - Linear01Depth(i.pos.z);
                float intersect = 0;

                if(diff > 0)
                    intersect = 1 - smoothstep(0, _ProjectionParams.w * _FadeLength, diff);

                //fresnel
                // apply silouette equation
				// based on how close normal is to being orthogonal to view vector
				// dot product is smaller the smaller the angle bw the vectors is
				// close to edge = closer to 0
				// far from edge = closer to 1
				float edgeFactor = abs(dot(i.viewDir, i.normal));
                edgeFactor = saturate(1 - edgeFactor) * _EdgeLength;
                edgeFactor = pow(edgeFactor, _EdgeExponent);

                intersect = max(intersect, edgeFactor);

				float4 texColor = tex2D(_MainTex, i.uv.xy);

                half4 glowColor = _Color; //half4(lerp(_Color.rgb, _GlowColor, pow(intersect, 4)), 1);
                if (intersect > _EdgeThreshold) {
                    glowColor = _GlowColor;
                }

                half4 col = glowColor;//_Color * _Color.a + glowColor;
                //col.a = _Color.a;

                return col;
            }


            ENDCG
        }
    }
}