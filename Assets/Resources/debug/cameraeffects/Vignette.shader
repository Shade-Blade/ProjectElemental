Shader "CameraEffect/Vignette" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1.0,1.0,1.0,1.0)
        _Power("Power", float) = 1
        _Offset("Offset", Range(0,2)) = 1
    }

    SubShader {

        CGINCLUDE
            #include "UnityCG.cginc"

            struct VertexData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            Texture2D _MainTex;
            SamplerState point_clamp_sampler;
            float4 _MainTex_TexelSize;

            v2f vp(VertexData v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        ENDCG

        Pass {
            CGPROGRAM
            #pragma vertex vp
            #pragma fragment fp

            half4 _Color;
            float _Power;    
            float _Offset;          

            half4 fp(v2f i) : SV_Target {
                half4 col = _MainTex.Sample(point_clamp_sampler, i.uv);

                half x = pow(abs(i.uv.x * 2 - 1), _Power);
                half y = pow(abs(i.uv.y * 2 - 1), _Power);

                half z = x + y + _Offset;

                half4 output = col;

                output = lerp(col, _Color, clamp(z,1,2) - 1);

                return output;
            }
            ENDCG
        }

        /*
        Pass {
            CGPROGRAM
            #pragma vertex vp
            #pragma fragment fp

            fixed4 fp(v2f i) : SV_Target {
                return _MainTex.Sample(point_clamp_sampler, i.uv);
            }
            ENDCG
        }
        */
    }
}