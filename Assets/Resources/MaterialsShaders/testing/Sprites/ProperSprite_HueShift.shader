Shader "Custom/ProperSprite_HueShift" {
	Properties{
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5
		_OcclusionColor("Occlusion Color", Color) = (0,0,1,1)
		_BacklightBorder("Backlight Border", float) = 0.5
		[MaterialToggle] _BidirectionalLight("Bidirectional Light", Float) = 0	//light from behind is counted

		_HueShift("Hue Shift", Range(-360,360)) = 0.0 
			//shift values:
			//0 = normal
			//150(210) = split complement
			//180 = complement (not the same as inverted)
			//120(240) = triad right
		_SatBoost("Saturation Boost", Float) = 0.0
		_ValBoost("Value Boost", Float) = 0.0
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

		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}

		//#pragma vertex vert
		//#pragma fragment frag

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		fixed _Cutoff;
		
		float4 _MainTex_TexelSize;
		float _BacklightBorder;

		float _HueShift;
		float _SatBoost;
		float _ValBoost;
		
		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;

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
			float3 color = RGBToHSV(c.rgb);

			color.x = color.x + _HueShift/360;
			color.y *= (1 + _SatBoost);
			color.z *= (1 + _ValBoost);

			float3 newColor = HSVToRGB(color);
			c.r = newColor.r;
			c.g = newColor.g;
			c.b = newColor.b;

			o.Albedo = c.rgb;
			o.Alpha = c.a;
			clip(o.Alpha - _Cutoff);
		}
		ENDCG
	}

	FallBack "Diffuse"
}