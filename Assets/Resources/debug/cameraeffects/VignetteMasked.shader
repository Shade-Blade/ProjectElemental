Shader "CameraEffect/VignetteMasked" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        [HDR]
        _Color("Color", Color) = (1.0,1.0,1.0,1.0)
        [HDR]
        _ColorB("ColorB", Color) = (1.0,1.0,1.0,1.0)
        _Power("Power", float) = 1
        _Offset("Offset", float) = 1
        _MaskPower("Mask Power", float) = 1
        _MaskScale("Mask Scale", float) = 1
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
            SamplerState point_repeat_sampler;
            float4 _MainTex_TexelSize;

            Texture2D _MaskTex;

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
            half4 _ColorB;
            float _Power;    
            float _Offset;     
            float _MaskPower;         
            float _MaskScale;         
 
            half4 fp(v2f i) : SV_Target {
                half4 col = _MainTex.Sample(point_clamp_sampler, i.uv);

                float px = i.uv.x;
                float py = i.uv.y * _MainTex_TexelSize.w / _MainTex_TexelSize.z;
                half4 mask = _MaskTex.Sample(point_repeat_sampler, float2(px, py) * _MaskScale);

                half x = pow(abs(i.uv.x * 2 - 1), _Power);
                half y = pow(abs(i.uv.y * 2 - 1), _Power);

                half z = x + y + _Offset + mask.r * _MaskPower;

                half4 output = col;

                half lerpCoeff = clamp(z,1,2) - 1;
                output = lerp(col, lerp(_Color, _ColorB, lerpCoeff), lerpCoeff);

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