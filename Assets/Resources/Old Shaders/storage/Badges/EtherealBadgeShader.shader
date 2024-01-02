Shader "Unlit/EtherealBadgeShader"
{
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		NoiseTex("Noise Texture", 2D) = "white" {}
		NoiseBlack("Black Value", Float) = 1.0 //multiplier for black
		NoiseWhite("White Value", Float) = 1.2 //multiplier for white
		UVMult("UV Mult", Float) = 4.0
		XMult("X Mult", Float) = 1.0
		YMult("Y Mult", Float) = 1.0
		//[PerRendererData]_Color("BadgeColor", Color) = (1.0,1.0,1.0,1.0)
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
		float NoiseBlack;
		float UVMult;
		float XMult;
		float YMult;

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

			float2 newUV = i.uv * UVMult;
			newUV.x += _Time.y * XMult;
			newUV.y += _Time.y * YMult;
			half4 noisecol = tex2D(NoiseTex, newUV);

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
			texcol.r *= i.color.r;
			texcol.g *= i.color.g;
			texcol.b *= i.color.b;

			texcol.r = texcol.r * (1 - contrastVal) + contrastVal;
			texcol.g = texcol.g * (1 - contrastVal) + contrastVal;
			texcol.b = texcol.b * (1 - contrastVal) + contrastVal;

			float value = (noisecol.rgb * (NoiseWhite - NoiseBlack)) + NoiseBlack;
			//texcol.rgb *= value;
			texcol.a *= value;
			texcol.rgb *= texcol.a;

			return texcol;
		}
		ENDCG

			}
		}
			Fallback "VertexLit"
}
