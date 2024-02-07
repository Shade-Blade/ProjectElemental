Shader "Custom/ProperSprite_PowerOverlay" {
	Properties{
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5
		_BacklightBorder("Backlight Border", float) = 0.5
		[MaterialToggle] _BidirectionalLight("Bidirectional Light", Float) = 0	//light from behind is counted

		_OverlayColor("Overlay Color", Color) = (0,0,0,0)
		_OverlayTex("Overlay Texture", 2D) = "white" {}
		_PerPixelScale("Overlay Pixel Scale", float) = 1
		_XOffset("X Offset", Range(0,1)) = 1	//cyclic so values outside 0,1 are equivalent to something in that range
		
		_TimeScale("Time Scale", float) = 1
		//_PTimeScale("Pulse Time Scale", float) = 1
	}
	SubShader {
		Tags
		{
			"Queue" = "Transparent-100"
			"RenderType" = "TransparentCutout"
		}
		LOD 200

		Cull Off
		ZWrite On
		Blend One OneMinusSrcAlpha

		/*
		Pass {
			Cull Off
			ZWrite On
			Blend One OneMinusSrcAlpha
		}
		*/
			

		/*
		Pass {
			Tags
			{
				"Queue" = "Transparent"
			}
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			Color [_OcclusionColor]
				
			Stencil {
				Ref 3
				Comp Greater
				Fail replace
				Pass replace
			}
				
		}

			
		Stencil {
			Ref 4
			Comp always
			Pass replace
			ZFail keep
		}
		*/
			
			

		CGPROGRAM
		// Lambert lighting model, and enable shadows on all light types
		#pragma surface surf SimpleLambert addshadow fullforwardshadows
		//#pragma debug

		//note: lambert has some extra features this one is missing?
		//can't use skybox reflections as well as antilight since it uses the light alpha channel while the unity data structures don't have light alpha
		//but it doesn't seem that the lighting difference matters?
		//sprites don't get skybox reflections and the ambient light seems to work properly still

		float _BidirectionalLight;

		half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
			half3 realNormal = s.Normal;

			//rotation matrix
			//half angle = _AngleOffset;
			//half3x3 rotMatrix = half3x3(half3(cos(angle), sin(angle), 0), half3(-sin(angle), cos(angle), 0), half3(0,0,1));
			//realNormal = mul(rotMatrix, realNormal);

			half NdotL = dot (realNormal, lightDir);
            half4 c;

			if (_LightColor0.a == 0) {
				atten = -atten;	//Draw some anti-light ;)
			}

			if (_BidirectionalLight == 1 || s.Gloss == 1) {
				NdotL = abs(NdotL);
			}
			half k = max(NdotL, 0);

	        c.rgb = s.Albedo * k * atten * _LightColor0.rgb;
            c.a = s.Alpha;
            return c;
        }

		//#pragma vertex vert
		//#pragma fragment frag

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		fixed _Cutoff;
		
		float4 _MainTex_TexelSize;
		float _BacklightBorder;

		sampler2D _OverlayTex;
		fixed4 _OverlayColor;
		float _PerPixelScale;

		float _TimeScale;
		float _XOffset;
		//float _PTimeScale;

		
		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 a = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 b = fixed4(0,0,0,0);
			if (a.a != 0) {
				float timeFactor = _Time.x * _TimeScale;

				float2 buv = IN.uv_MainTex;
				buv.x /= _MainTex_TexelSize.x * _PerPixelScale;
				buv.y /= _MainTex_TexelSize.y * _PerPixelScale;
				buv.x = buv.x + _XOffset;
				buv.y = buv.y - timeFactor;
				b = tex2D(_OverlayTex, buv) * _OverlayColor;
				b *= 0.8 + 0.2 * sin(timeFactor * 1.2);
			}
			//fixed4 c = lerp(a, b, _OverlayOpacity) * IN.color;
			fixed4 c = a * IN.color;
			//c += b;

			o.Emission = b;

			//sobel filter (outline)
			float d = _MainTex_TexelSize.xy * _BacklightBorder;

			half a1 = tex2D(_MainTex, IN.uv_MainTex + d * float2(-1, -1)).a;
			half a2 = tex2D(_MainTex, IN.uv_MainTex + d * float2(0, -1)).a;
			half a3 = tex2D(_MainTex, IN.uv_MainTex + d * float2(+1, -1)).a;

			half a4 = tex2D(_MainTex, IN.uv_MainTex + d * float2(-1,  0)).a;
			half a6 = tex2D(_MainTex, IN.uv_MainTex + d * float2(+1,  0)).a;

			half a7 = tex2D(_MainTex, IN.uv_MainTex + d * float2(-1, +1)).a;
			half a8 = tex2D(_MainTex, IN.uv_MainTex + d * float2(0, +1)).a;
			half a9 = tex2D(_MainTex, IN.uv_MainTex + d * float2(+1, +1)).a;

			float gx = -a1 - a2 * 2 - a3 + a7 + a8 * 2 + a9;
			float gy = -a1 - a4 * 2 - a7 + a3 + a6 * 2 + a9;
			
			float w = sqrt(gx * gx + gy * gy) / 4;

			//use the gloss value incorrectly I guess
			if (w > 0) {
				o.Gloss = 1;
			}

			//set hue, cap saturation and value
			

			o.Albedo = c.rgb;
			o.Alpha = c.a;
			clip(o.Alpha - _Cutoff);
		}
		ENDCG
	}

	FallBack "Diffuse"
}