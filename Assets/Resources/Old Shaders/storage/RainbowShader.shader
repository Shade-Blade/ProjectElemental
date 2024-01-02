Shader "Unlit/RainbowShader"
{
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	//_Color("BadgeColor", Color) = (1.0,1.0,1.0,1.0)
		TimeFactor("Time Factor", Float) = 1.5
		XFactor("X Factor", Float) = 30
		YFactor("Y Factor", Float) = -30
		RGBFactor("RGB Factor", Float) = 1.3
		AFactor("A Factor (darkness)", Float) = 0.0
		BFactor("B Factor (lightness)", Float) = 0.0
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
		//#pragma multi_compile_PIXELSNAP_ON​

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		fixed4 color;

		float TimeFactor;
		float XFactor;
		float YFactor;
		float RGBFactor;
		float AFactor;
		float BFactor;

		struct v2f {
			float4  pos : SV_POSITION;
			float2  uv : TEXCOORD0;
		};

		float4 _MainTex_ST;

		v2f vert(appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			return o;
		}

		half4 frag(v2f i) : COLOR
		{
			half4 texcol = tex2D(_MainTex, i.uv);

			//color
			//value = time + uv x + uv y (scale all three of these values and modulo them too)
			//giving X and Y different multipliers causes wave angle to change
			#ifdef PIXELSNAP_ON
				i.pos = UnityPixelSnap(i.pos);
			#endif

			float hue = _Time.y * TimeFactor + (i.uv.x * XFactor + i.uv.y * YFactor);
			while (hue < 0) {
				hue += 3;
			}
			while (hue > 3) {
				hue -= 3;
			}

			fixed4 color;
			//0-0.5 (r +g)
			//0.5-1 (r- g)
			//1-1.5 (g +b)
			//1.5-2 (g- b)
			//2-2.5 (b r+)
			//2.5-3 (b- r)
			if (hue < 0.5) {
				color.r = 1;
				color.g = hue * 2;
				color.b = 0;
			}
			else if (hue > 0.5 && hue < 1.0) {
				color.r = 2 - (hue * 2);
				color.g = 1;
				color.b = 0;
			}
			else if (hue > 1.0 && hue < 1.5) {
				color.r = 0;
				color.g = 1;
				color.b = (hue * 2) - 2;
			}
			else if (hue > 1.5 && hue < 2.0) {
				color.r = 0;
				color.g = 4 - (hue * 2);
				color.b = 1;
			}
			else if (hue > 2.0 && hue < 2.5) {
				color.r = (hue * 2) - 4;
				color.g = 0;
				color.b = 1;
			}
			else if (hue > 2.5 && hue < 3.0) {
				color.r = 1;
				color.g = 0;
				color.b = 6 - (hue * 2);
			}

			//modify color
			color.rgb *= (1.0 - AFactor);
			color.r = color.r * (1.0 - BFactor) + BFactor;
			color.g = color.g * (1.0 - BFactor) + BFactor;
			color.b = color.b * (1.0 - BFactor) + BFactor;

			//texcol.rgb *= _RendererColor;
			texcol.rgb *= texcol.a;
			half cval = texcol.rgb *  RGBFactor;
			texcol.r = cval * color.r;
			texcol.g = cval * color.g; //force rgb to be the same
			texcol.b = cval * color.b;

			return texcol;
		}

		ENDCG

			}
	}
		Fallback "VertexLit"
}
