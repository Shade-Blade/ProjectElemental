Shader "Custom/Fire"
{
    Properties
    {
		[HDR]
        _BaseColor ("Color", Color) = (1,1,1,1)
		[HDR]
		_BaseColorB ("ColorB", Color) = (1,1,1,1)
		[HDR]
        _InnerColor ("InnerColor", Color) = (1,1,1,1)
		[HDR]
		_InnerColorB ("InnerColorB", Color) = (1,1,1,1)

		_NoiseFactor("Noise Factor", float) = 0.3
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        NoiseTex("Noise Texture", 2D) = "white" {}
    }
    SubShader{
		Pass {
			Tags { "QUEUE" = "Transparent" "IGNOREPROJECTOR" = "true" "RenderType" = "Transparent" "CanUseSpriteAtlas" = "true" "PreviewType" = "Plane" }

			ZWrite Off
			Cull Off
			Blend One OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			
			half4 _BaseColor;
			half4 _BaseColorB;
			half4 _InnerColor;
			half4 _InnerColorB;

			half _NoiseFactor;

			sampler2D _MainTex;
			half4 EdgeColor;
			float EdgeOffset;
			sampler2D NoiseTex;
			float NoiseDensity;
			float AFactor;

			struct v2f {
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
				float4  color : COLOR;
			};

			float4 _MainTex_ST;

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;
				return o;
			}

			void Unity_Ellipse_float(float2 UV, float Width, float Height, out float4 Out)
			{
				float d = length((UV * 2 - 1) / float2(Width, Height));
				Out = saturate((1 - d) / fwidth(d));
			}

			half4 frag(v2f i) : COLOR
			{
				half4 texcol = half4(1,1,1,1);//tex2D(_MainTex, i.uv);

				/*
				texcol.rgb *= texcol.a;
				texcol.r *= i.color.r;
				texcol.g *= i.color.g;
				texcol.b *= i.color.b;
				*/

				float4 test = float4(0,0,0,0);

				float2 newUV = i.uv + _Time.y * float2(0,-1);
				half4 noisecol = tex2D(NoiseTex, newUV);

				noisecol.r *= i.uv.y * i.uv.y * saturate(1.25 - 4 * abs(i.uv.x - 0.5)); //i.uv.y * i.uv.y * (1 + saturate(1 - 2 * i.uv.x));

				i.uv.y -= noisecol.r * _NoiseFactor;

				Unity_Ellipse_float(i.uv, 0.225, 0.225, test);

				if (test.a > 0.5) {
					float a = i.uv.y;

					a *= 2;

					a = clamp(a,0,1);
					fixed4 interColor = _BaseColor * a + _BaseColorB * (1 - a);

					test.r = interColor.r;
					test.g = interColor.g;
					test.b = interColor.b;
				} else {
					Unity_Ellipse_float(i.uv, 0.3, 0.3, test);


					if (test.a > 0.5) {
						float b = i.uv.y;

						b *= 2;
						
						b = clamp(b,0,1);
						fixed4 interColorB = _InnerColor * b + _InnerColorB * (1 - b);

						test.r = interColorB.r;
						test.g = interColorB.g;
						test.b = interColorB.b;
					} 
					/*else {
						float c = i.uv.y;

						c *= 2;

						c = clamp(c,0,1);
						fixed4 interColor = _BaseColor * c + _BaseColorB * (1 - c);

						test.r = interColor.r;
						test.g = interColor.g;
						test.b = interColor.b;
						test.a = 0;
					}*/
				}

				return test;
			}

			ENDCG
		}
	}
	Fallback "VertexLit"
}
