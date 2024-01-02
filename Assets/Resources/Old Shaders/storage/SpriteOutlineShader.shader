Shader "Sprite/Sprite Outline"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = ""{}
		_Distance("Distance", Float) = 1
		_Color("Color", Color) = (1, 0, 0, 0)
		_ForceZeroAlpha("Force Zero Alpha", Float) = 0
		_SoftEdges("Soft Edges", Float) = 0
	}

		CGINCLUDE

#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		float _Distance;
		half4 _Color;
		float _ForceZeroAlpha;
		float _SoftEdges;

		half4 frag(v2f_img i) : SV_Target
		{
			half4 source = tex2D(_MainTex, i.uv);
			half a0 = source.a;

			//force zero alpha prevents drawing outlines when alpha is nonzero
			if (a0 != 0 && _ForceZeroAlpha == 0) {
				return source;
			}

			if (a0 == 0) {
				source.rgb = _Color;
				source.a = 0;
			}

			// Simple sobel filter for the alpha channel.
			float d = _MainTex_TexelSize.xy * _Distance;

			half a1 = tex2D(_MainTex, i.uv + d * float2(-1, -1)).a;
			half a2 = tex2D(_MainTex, i.uv + d * float2(0, -1)).a;
			half a3 = tex2D(_MainTex, i.uv + d * float2(+1, -1)).a;

			half a4 = tex2D(_MainTex, i.uv + d * float2(-1,  0)).a;
			half a6 = tex2D(_MainTex, i.uv + d * float2(+1,  0)).a;

			half a7 = tex2D(_MainTex, i.uv + d * float2(-1, +1)).a;
			half a8 = tex2D(_MainTex, i.uv + d * float2(0, +1)).a;
			half a9 = tex2D(_MainTex, i.uv + d * float2(+1, +1)).a;

			float gx = -a1 - a2 * 2 - a3 + a7 + a8 * 2 + a9;
			float gy = -a1 - a4 * 2 - a7 + a3 + a6 * 2 + a9;
			
			float w = sqrt(gx * gx + gy * gy) / 4;

			if (w > 0 || source.a > 0) {
				// Mix the contour color.

				half4 end = half4(lerp(source.rgb, _Color.rgb, w), 1);

				if (_SoftEdges != 0) {
					end.a = w;
				}

				return end;
			}
			else {
				source.a = 0;
				source.rgb = 0;
				return source;
			}

		}

			ENDCG

			Subshader
		{
			Pass
			{
				ZTest Always Cull Off ZWrite Off
				Blend One OneMinusSrcAlpha
				Fog { Mode off }
				CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				ENDCG
			}
		}
}