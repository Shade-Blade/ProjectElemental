Shader "Unlit/SparklingShader"
{
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		NoiseTex("Noise Texture", 2D) = "white" {}
		NoiseDensity("Noise Density", Float) = 1.0
		NoiseWhite("White Value", Float) = 1.2 //multiplier for white
		TimeFactor("Time Factor", Float) = 1.0
		EdgeWidth("Outline Width", Float) = 1.0
		EdgeColor("Outline Color", Color) = (1.0,1.0,1.0,1.0)
		EdgeTrim("Edge Trim", Float) = 0.2
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
		sampler2D NoiseTex;
		fixed4 _Color;

		float NoiseWhite;
		float NoiseDensity;
		float TimeFactor;
		float EdgeWidth;
		half4 EdgeColor;
		float EdgeTrim;

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


			float timeValue = _Time.y * TimeFactor;
			timeValue = frac(timeValue);
			timeValue *= (1 - 2 * EdgeTrim);
			timeValue += EdgeTrim;
			float2 newUV = i.uv * NoiseDensity;
			half4 noisecol = tex2D(NoiseTex, newUV);			

			//compare stuff
			//time value - noisecol > 0 and < edgeoffset (or > -1 and < edgeoffset - 1)

			//if sparkle...
			float S = timeValue - noisecol;
			while (S > EdgeWidth) {
				S -= 1;
			}
			while (S < 0) {
				S += 1;
			}
			if (S > 0 && S < EdgeWidth) {
				texcol.rgb *= NoiseWhite;
				texcol.r *= EdgeColor.r;
				texcol.g *= EdgeColor.g;
				texcol.b *= EdgeColor.b;
			}

			return texcol;
		}
		ENDCG

			}
		}
			Fallback "VertexLit"
}
