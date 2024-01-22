Shader "CameraEffect/VignetteMaskedDark" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        [HDR]
        _Color("Color", Color) = (1.0,1.0,1.0,1.0)
        [HDR]
        _ColorB("ColorB", Color) = (1.0,1.0,1.0,1.0)
        [HDR]
        _ColorM("Color Minus", Color) = (1.0,1.0,1.0,1.0)
        [HDR]
        _ColorMB("Color Minus B", Color) = (1.0,1.0,1.0,1.0)
        _Power("Power", float) = 1
        _Offset("Offset", float) = 1
        _MaskPower("Mask Power", float) = 1
        _MaskScaleX("Mask Scale X", float) = 1  //Make it an integer or else the seam will be visible (Radial X coords go from 0 to 1 because I scaled it)
        _MaskScaleY("Mask Scale Y", float) = 1
        _MaskScrollX("Mask Scroll X", float) = 1
        _MaskScrollY("Mask Scroll Y", float) = 1
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
            half4 _ColorM;
            half4 _ColorMB;
            float _Power;    
            float _Offset;     
            float _MaskPower;         
            float _MaskScaleX;           
            float _MaskScaleY;          
            float _MaskScrollX;           
            float _MaskScrollY;    
 
            half4 fp(v2f i) : SV_Target {
                half4 col = _MainTex.Sample(point_clamp_sampler, i.uv);


                float px = (i.uv.x * 2 - 1) * _MainTex_TexelSize.z;
                float py = (i.uv.y * 2 - 1) * _MainTex_TexelSize.w;

                half rx = atan2(py, px) * _MaskScaleX / 6.283185307179586;

                half x = pow(abs(i.uv.x * 2 - 1), _Power);
                half y = pow(abs(i.uv.y * 2 - 1), _Power);

                half ry = (x + y) * _MaskScaleY;

                half xscr = _MaskScrollX * _Time.y;
                half yscr = _MaskScrollY * _Time.y;

                half4 mask = _MaskTex.Sample(point_repeat_sampler, float2(rx + xscr, ry + yscr));
                half4 maskB = _MaskTex.Sample(point_repeat_sampler, float2(rx - xscr, ry + yscr));

                mask = (mask + maskB) / 2;

                half z = x + y + _Offset + mask.r * _MaskPower;

                half4 output = col;
                
                half lerpCoeff = clamp(z,1,2) - 1;
                output = lerp(col, lerp(_Color - _ColorM, _ColorB - _ColorMB, lerpCoeff), clamp(z,1,2) - 1);

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