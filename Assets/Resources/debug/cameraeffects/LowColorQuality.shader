Shader "Hidden/LowColorQuality" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _RedColorCount("Red", float) = 5
        _GreenColorCount("Green", float) = 5
        _BlueColorCount("Blue", float) = 5
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

            float _Spread;
            int _RedColorCount, _GreenColorCount, _BlueColorCount;

            fixed4 fp(v2f i) : SV_Target {
                float4 col = _MainTex.Sample(point_clamp_sampler, i.uv);

                uint x = i.uv.x * _MainTex_TexelSize.z;
                uint y = i.uv.y * _MainTex_TexelSize.w;

                float4 output = col;

                output.r = floor((_RedColorCount - 1.0f) * output.r + 0.5) / (_RedColorCount - 1.0f);
                output.g = floor((_GreenColorCount - 1.0f) * output.g + 0.5) / (_GreenColorCount - 1.0f);
                output.b = floor((_BlueColorCount - 1.0f) * output.b + 0.5) / (_BlueColorCount - 1.0f);

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