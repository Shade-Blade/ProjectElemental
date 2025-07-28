//shader that makes stuff appear in front
//normal shader doesn't let me do that

Shader "Unlit/BattleZoom"
{
    Properties
    {
		[PerRendererData]  _MainTex("Sprite Texture", 2D) = "white" { }
		 _Color("Color", Color) = (1.000000,1.000000,1.000000,1.000000)
         _ZoomX("Zoom X", float) = 0.5
         _ZoomY("Zoom Y", float) = 0.5
         _ZoomRatio("Zoom Ratio", float) = 0.75
         _ZoomLevel("Zoom Level", float) = 0
         _Rotate("Rotate", float) = 0
         _PointCount ("PointCount", float) = 5            //note: fractional numbers lead to broken stars
         [HDR]
         _ColorMult("Color Mult", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Overlay+1"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
				float4  color : COLOR;
            };

            sampler2D _MainTex;
			float4 _Color;
            float4 _MainTex_ST;

            float _ZoomX;
            float _ZoomY;
            float _ZoomLevel;
            float _ZoomRatio;
            float4 _ColorMult;

            float _Rotate;

            float _BattleFadeProgress;
            float2 _BattleFadeLocation;

            float _PointCount;

            v2f vert (appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;
				//o.color *= _Color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture

                if (_ZoomLevel * _BattleFadeProgress == 0) {
                    return half4(0,0,0,1);
                }

                float2 newUV = i.uv;

                newUV.x -= _BattleFadeLocation.x - 0.5;//_ZoomX - 0.5;
                newUV.y -= _BattleFadeLocation.y - 0.5;//_ZoomY - 0.5;

                newUV.x -= 0.5;
                newUV.y -= 0.5;


                newUV.xy *= (1 / (_ZoomLevel * _BattleFadeProgress));
                newUV.y *= (_ZoomRatio);

                float2 rotated = float2(cos(_Rotate * _BattleFadeProgress) * newUV.x + sin(_Rotate * _BattleFadeProgress) * newUV.y, sin(_Rotate * _BattleFadeProgress) * newUV.x + -cos(_Rotate * _BattleFadeProgress) * newUV.y);

                newUV.x = rotated.x;
                newUV.y = rotated.y;


                newUV.x += 0.5;
                newUV.y += 0.5;

                /*
                newUV.x += _ZoomX;
                newUV.y += _ZoomY;

                newUV.y *= (_ZoomRatio);
                newUV.x += 0.5 * _ZoomLevel;
                newUV.y += 0.5 * _ZoomLevel;// + ((1 - _ZoomRatio) / 2);
                */                

                newUV = saturate(newUV);

                /*
                half4 col = tex2D(_MainTex, newUV) * _ColorMult;
				col.rgb *= col.a;
				col.r *= i.color.r;
				col.g *= i.color.g;
				col.b *= i.color.b;
                */

                fixed4 col = fixed4(1,1,1,1);   //tex2D(_MainTex, i.uv);
				col.rgb *= col.a;
				col.r *= i.color.r;
				col.g *= i.color.g;
				col.b *= i.color.b;
                //col.a *= i.color.a;

                fixed radius = 0.25;
                fixed inradius = lerp(0.175,radius, _BattleFadeProgress);

                half angle = 0.75 + (atan2(newUV.y - 0.5, newUV.x - 0.5) / 6.283185307179586);
                half b = frac(angle * _PointCount);
                half c = 2 * abs(b - 0.5);
                half d = lerp(inradius,radius,pow(c,2));
                half dB = lerp(inradius,radius,pow(c,4));

                if (length(newUV - half2(0.5,0.5)) > dB * 1.6) {
                    col.rgb *= 0;
                }

                if (length(newUV - half2(0.5,0.5)) > d * 1.3) {
                    col.rgb *= 0.5;
                }

                if (length(newUV - half2(0.5,0.5)) < d) {
                    col.a = 0;
                }

                return col;
            }
            ENDCG
				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off
				ZTest Always
        }
    }
}
