Shader "VFX/FireParticle"
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

		_WidthA("Width A", float) = 0.3
		_WidthA2("Width A2", float) = 0.6
		_WidthB("Width B", float) = 0.15
		_WidthB2("Width B2", float) = 0.3
		
		_InterpScale("InterpScale", float) = 2

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

			half _WidthA;
			half _WidthA2;
			half _WidthB;
			half _WidthB2;
			
			half _InterpScale;

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

				//Billboard thing
				//Probably don't use this?
				/*
				o.pos = mul(UNITY_MATRIX_P, 
					mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
					+ float4(v.vertex.x, v.vertex.y, 0.0, 0.0)
					* float4(1, 1, 1.0, 1.0));
				*/

				return o;
			}

			void Ovoid(float2 UV, float width, float height, float topheight, out float4 Out) {
				float2 k = (UV * 2 - 1);
				float d = 0;
				if (k.y > 0) {
					k = k / float2(width, topheight);
					d = pow(pow(abs(k.x), 1.5) + pow(abs(k.y), 1.5), 0.666);
				} else {
					k = k / float2(width, height);
					d = length(k);
				}
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

				//Rotate
				i.uv = float2(i.uv.y, i.uv.x);

				float2 newUV = i.uv + _Time.y * float2(0,-1);

				//offset looks wrong sideways
				float offset = 0.0005 * (i.pos.y - i.pos.z - i.pos.x);
				offset = 2 * abs(frac(offset * 0.5) - 0.5);

				newUV.x += offset;

				half4 noisecol = tex2D(NoiseTex, newUV);

				noisecol.r *= i.uv.y * i.uv.y * saturate(1.25 - 4 * abs(i.uv.x - 0.5)); //i.uv.y * i.uv.y * (1 + saturate(1 - 2 * i.uv.x));

				i.uv.y -= noisecol.r * _NoiseFactor;

				Ovoid(i.uv, _WidthB, _WidthB, _WidthB2, test);

				test.rgb = _BaseColor;
				if (test.a > 0.5) {
					float a = i.uv.y;

					a = (a - 0.5) * _InterpScale + 0.5;

					a = saturate(a);
					fixed4 interColor = _BaseColor * a + _BaseColorB * (1 - a);

					test.r = interColor.r;
					test.g = interColor.g;
					test.b = interColor.b;
				} else {
					Ovoid(i.uv, _WidthA, _WidthA, _WidthA2, test);

					if (test.a > 0.5) {
						float b = i.uv.y;

						b *= (b - 0.5) * _InterpScale + 0.5;
						
						b = saturate(b);
						fixed4 interColorB = _InnerColor * b + _InnerColorB * (1 - b);

						test.r = interColorB.r;
						test.g = interColorB.g;
						test.b = interColorB.b;
					} 
				}

				return test;
			}

			ENDCG
		}
	}
	Fallback "VertexLit"
}
