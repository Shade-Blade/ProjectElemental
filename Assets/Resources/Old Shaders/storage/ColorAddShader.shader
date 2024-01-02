Shader "Unlit/ColorAddShader" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		Power("Power",Float) = 1.0
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
	//fixed4 _Color;

	struct v2f {
		float4  pos : SV_POSITION;
		float2  uv : TEXCOORD0;
		float4  color : COLOR;
	};

	float4 _MainTex_ST;
	float Power;

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

		texcol.r += i.color.r * Power;
		texcol.g += i.color.g * Power;
		texcol.b += i.color.b * Power;
		texcol.rgb *= texcol.a;

		return texcol;
	}
	ENDCG
}
	}
		Fallback "VertexLit"
}