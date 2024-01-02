//does nothing

Shader "Unlit/DissolveSpriteShader" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		EdgeColor("Edge Color", Color) = (1.0,1.0,1.0,1.0)
		EdgeOffset("Edge Offset", Float) = 0.1
		NoiseTex("Noise Texture", 2D) = "white" {}
		NoiseDensity("Noise Density", Float) = 1.0
		[PerRendererData]
		AFactor("AFactor", Range(-1,1)) = 0.0 //negative numbers are for waiting for glow outline to disappear
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

			sampler2D _MainTex;
			fixed4 EdgeColor;
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

			half4 frag(v2f i) : COLOR
			{
				half4 texcol = tex2D(_MainTex, i.uv);

				texcol.rgb *= texcol.a;
				texcol.r *= i.color.r;
				texcol.g *= i.color.g;
				texcol.b *= i.color.b;

				//this probably doesn't help much
				if (AFactor < 1) {
					float t = 1 - AFactor;
					float2 newUV = i.uv * NoiseDensity;
					half4 noisecol = tex2D(NoiseTex, newUV);
					float a = noisecol.rgb;

					//compare t to noise pixels
					//t < a = normal
					if (t < a || texcol.a == 0) {
						return texcol;
					}
					//t < a + edge = edge color
					if (t < a + EdgeOffset) {
						return EdgeColor;
					}

					return half4(0,0,0,0);
				}
				return texcol;
			}
			ENDCG
		}
	}
	Fallback "VertexLit"
}