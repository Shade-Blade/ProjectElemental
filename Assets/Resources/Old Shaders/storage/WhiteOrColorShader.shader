//White or color
//Forces all colors to have the color's hue

Shader "Unlit/WhiteOrColorShader" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		//[PerRendererData]_Color("BadgeColor", Color) = (1.0,1.0,1.0,1.0)
		SatPeak("Saturation Peak", Float) = 0.5
		SatMult("Saturation Multiplier", Float) = 0.0
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
		float SatPeak;
		float SatMult;

		struct v2f {
			float4  pos : SV_POSITION;
			float2  uv : TEXCOORD0;
			float4  color : COLOR;
		};

		float4 _MainTex_ST;

		float3 hue2rgb(float hue) {
			hue = frac(hue); //only use fractional part of hue, making it loop
			float r = abs(hue * 6 - 3) - 1; //red
			float g = 2 - abs(hue * 6 - 2); //green
			float b = 2 - abs(hue * 6 - 4); //blue
			float3 rgb = float3(r, g, b); //combine components
			rgb = saturate(rgb); //clamp between 0 and 1
			return rgb;
		}

		float3 hsv2rgb(float3 hsv)
		{
			float3 rgb = hue2rgb(hsv.x); //apply hue
			rgb = lerp(1, rgb, hsv.y); //apply saturation
			rgb = rgb * hsv.z; //apply value
			return rgb;
		}

		float3 rgb2hsv(float3 rgb)
		{
			float maxComponent = max(rgb.r, max(rgb.g, rgb.b));
			float minComponent = min(rgb.r, min(rgb.g, rgb.b));
			float diff = maxComponent - minComponent;
			float hue = 0;
			if (maxComponent == rgb.r) {
				hue = 0 + (rgb.g - rgb.b) / diff;
			}
			else if (maxComponent == rgb.g) {
				hue = 2 + (rgb.b - rgb.r) / diff;
			}
			else if (maxComponent == rgb.b) {
				hue = 4 + (rgb.r - rgb.g) / diff;
			}
			if (diff == 0) {
				hue = 0; //fix div by 0
			}
			hue = frac(hue / 6);
			float saturation = diff / maxComponent;
			float value = maxComponent;
			return float3(hue, saturation, value);
		}


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
			half4 original = texcol;

			float a = texcol.a;

			float3 setColor = rgb2hsv(i.color);

			//get saturation and value				
			float3 color = rgb2hsv(texcol);

			//set hue, cap saturation and value
			float diff = color.x - setColor.x;
			if (diff > 0.5) {
				//color.z = 0.1;
				diff = 1 - diff;
			}
			if (diff < -0.5) {
				//color.z = -diff;
				diff = -1 - diff;
			}

			float newSat = SatPeak - abs(diff);
			if (newSat < 0) {
				newSat = 0;
			}
			//newSat *= 1 / SatPeak;
			newSat *= SatMult;
			color.x = setColor.x;
			color.y *= (newSat/SatPeak);
			//color.z *= (1 + ValBoost);

			float3 newColor = hsv2rgb(color);
			texcol.r = newColor.r;
			texcol.g = newColor.g;
			texcol.b = newColor.b;

			texcol.a = a;
			texcol.rgb *= texcol.a;

			return texcol;
		}
		ENDCG

			}
		}
			Fallback "VertexLit"
}