Shader "Custom/ProperSpriteOutlineNormalMap" {
	Properties{
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData]_OutlineMap("Outline Map", 2D) = "white" {}
		[PerRendererData]_NormalMap("Normal Map", 2D) = "white" {}
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5
		_OcclusionColor("Occlusion Color", Color) = (0,0,1,1)
		_BacklightBorder("Backlight Border", float) = 0.5
		_DotScale("Dot Scale", float) = 0.3
		_FalloffDot("Falloff Dot", float) = 0.4
		[MaterialToggle] _BidirectionalLight("Bidirectional Light", Float) = 0	//light from behind is counted
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
		float _FalloffDot;
		float _DotScale;

		half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
			half3 realNormal = s.Normal;

			//rotation matrix
			//half angle = _AngleOffset;
			//half3x3 rotMatrix = half3x3(half3(cos(angle), sin(angle), 0), half3(-sin(angle), cos(angle), 0), half3(0,0,1));
			//realNormal = mul(rotMatrix, realNormal);

			half3 altLightDir = lightDir;


			half NdotL = dot (realNormal, altLightDir);

			if (_BidirectionalLight == 1 || s.Gloss == 1) {
				//wacky thing
				//half3 newldir = normalize(mul(unity_WorldToObject, half4(lightDir, 0)));
				//half3 oldnormal = normalize(mul(unity_WorldToObject, half4(realNormal, 0)));

				//lazy thing
				//half3 newdot = 0.35 + 0.65 * dot (realNormal, altLightDir);

				if (NdotL < _FalloffDot) {
					NdotL = _FalloffDot + _DotScale * (NdotL - _FalloffDot);
				}

				//NdotL = newdot;
			}

            half4 c;

			if (_LightColor0.a == 0) {
				atten = -atten;	//Draw some anti-light ;)
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
		sampler2D _OutlineMap;
		sampler2D _NormalMap;
		fixed _Cutoff;
		
		float4 _MainTex_TexelSize;
		float _BacklightBorder;
		
		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
			fixed4 outline = tex2D(_OutlineMap, IN.uv_MainTex);					
			half3 mapnormal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));

			//???
			//w = 0 negates translation?
			//normalize negates scale?
			//so only rotation is left (?????)
			half3 newnormal = normalize(mul(unity_ObjectToWorld, half4(mapnormal, 0)));

			o.Normal = newnormal;

			//use the gloss value incorrectly I guess
			if (outline.g > 0) {
				o.Gloss = 1;
			}

			o.Albedo = c.rgb;
			o.Alpha = c.a;
			clip(o.Alpha - _Cutoff);
		}
		ENDCG
	}

	FallBack "Diffuse"
}