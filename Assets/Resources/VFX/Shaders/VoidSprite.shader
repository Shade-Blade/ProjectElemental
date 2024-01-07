// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VFX/VoidSprite"
{
	//a copy paste of the default sprite shader with stuff added

	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

		_TimeScaleA ("Time Scale", float) = 2
        _ScreenTex ("Screen Texture", 2D) = "white" {}
        _LowEnd ("Low end", float) = -5
        _HighEnd ("High end", float) = 0.4
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};
			
			fixed4 _Color;

			sampler2D _ScreenTex;
			float4 _ScreenTex_ST;

			half _TimeScaleA;

			half _LowEnd;
			half _HighEnd;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				OUT.screenPos = ComputeScreenPos(OUT.vertex);

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			half4 frag(v2f IN) : SV_Target
			{				
				half4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c.rgb *= c.a;

				if (c.a > 0) {
					float2 textureCoordinate = IN.screenPos.xy / IN.screenPos.w;
					float aspect = _ScreenParams.x / _ScreenParams.y;
					textureCoordinate.x = textureCoordinate.x * aspect;
					textureCoordinate = TRANSFORM_TEX(textureCoordinate, _ScreenTex);

					//precompute some numbers
					float a1 = sin(_Time.x * _TimeScaleA);
					float a2 = sin(_Time.x * _TimeScaleA * 2.21);
					float a3 = sin(_Time.x * _TimeScaleA * 4.26);
					float a4 = sin(_Time.x * _TimeScaleA * 0.61);
					float a5 = cos(_Time.x * _TimeScaleA);
					float a6 = cos(_Time.x * _TimeScaleA * 2.62);
					float a7 = cos(_Time.x * _TimeScaleA * 4.12);
					float a8 = cos(_Time.x * _TimeScaleA * 0.57);


					//scroll RGB independently
					float2 texScrollR = textureCoordinate;
					float2 texScrollG = textureCoordinate;
					float2 texScrollB = textureCoordinate;

					float adiff = a3 * 5.1 + a2 * 1.1 + a1 * 0.52;
					float bdiff = a7 * 4.1 + a6 * 1.3 + a5 * 0.61;
					float cdiff = a2 * 4.1 + a6 * 1.3 + a5 * 0.61;
					float ddiff = a7 * 4.1 + a5 * 1.3 + a1 * 0.61;

					texScrollR.x += adiff + bdiff;
					texScrollR.y += bdiff + cdiff;
					texScrollG.x += cdiff + ddiff;
					texScrollG.y += ddiff + adiff;
					texScrollB.x += adiff + cdiff;
					texScrollB.y += bdiff + ddiff;

					//note: make the rgb channels independent for more chaotic scrolling?
					fixed4 colR = tex2D(_ScreenTex, texScrollR);
					fixed4 colG = tex2D(_ScreenTex, texScrollG);
					fixed4 colB = tex2D(_ScreenTex, texScrollB);

					half4 vec = half4(normalize(half3(colR.r, colG.g, colB.b)),1);

					//point one color forward the others back
					float ar = abs(vec.r);
					float ag = abs(vec.g);
					float ab = abs(vec.b);
					float am = max(ar, max(ag, ab));

					if (ar == am) {
						vec.r = ar;
					}
					if (ag == am) {
						vec.g = ag;
					}
					if (ab == am) {
						vec.b = ab;
					}

					vec.r = _LowEnd + (_HighEnd - _LowEnd) * vec.r;
					vec.g = _LowEnd + (_HighEnd - _LowEnd) * vec.g;
					vec.b = _LowEnd + (_HighEnd - _LowEnd) * vec.b;

					vec *= c.a;

					return vec;
				}

				return c;
			}
		ENDCG
		}
	}
}