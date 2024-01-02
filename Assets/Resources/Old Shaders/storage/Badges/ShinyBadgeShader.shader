Shader "Unlit/ShinyBadgeShader"
{
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		//[PerRendererData]_Color("BadgeColor", Color) = (1.0,1.0,1.0,1.0)
		Frequency ("Frequency", Float) = 1.0
		LineWidth ("Line Width", Float) = 0.2
		XMult ("X Multiplier", Float) = 1.0
		YMult ("Y Multiplier", Float) = -1.0
		Intensity ("Intensity", Float) = 2.0
		LowIntensity("Low Intensity", Float) = 1.25
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
		fixed4 _Color;

		float Frequency;
		float LineWidth;
		float XMult;
		float YMult;
		float Intensity;
		float LowIntensity;

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

			//input 100% saturation colors for badge color
			//then the shader will make the colors more accurate
			float contrastVal = 0;

			if (texcol.r > 0.9 || texcol.g > 0.9 || texcol.b > 0.9) {
				//decrease contrast
				contrastVal = 0.45;
			}
			else if (texcol.r > 0.8 || texcol.g > 0.8 || texcol.b > 0.8) {
				contrastVal = 0.15;
			}
			else if (texcol.r > 0.7 || texcol.g > 0.7 || texcol.b > 0.7) {
				contrastVal = 0.1;
			}

			//texcol.rgb *= _RendererColor;
			texcol.rgb *= texcol.a;
			if (texcol.a < 0.1) {
				texcol.a = 0;
			}
			texcol.r *= i.color.r;
			texcol.g *= i.color.g;
			texcol.b *= i.color.b;

			texcol.r = texcol.r * (1 - contrastVal) + contrastVal;
			texcol.g = texcol.g * (1 - contrastVal) + contrastVal;
			texcol.b = texcol.b * (1 - contrastVal) + contrastVal;

			//shine: (a line)
			//get our time value
			//float value = (_Time.y + i.uv.x * XMult + i.uv.y * YMult);
			float value = _Time.y * 1.5 + (i.uv.x * XMult + i.uv.y * YMult);
			value %= Frequency;
			while (value < 0) {
				value += Frequency;
			}
			if (value > Frequency - LineWidth) {
				texcol.rgb *= Intensity;
			}
			else {
				texcol.rgb *= LowIntensity;
			}

			return texcol;
		}
		ENDCG

			}
		}
			Fallback "VertexLit"
}
