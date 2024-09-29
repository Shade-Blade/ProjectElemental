// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//= SilhouetteIncluded + AutoOutlineNormals
//So it has all the features I need for sprites in general

Shader "Custom/ProperSpriteFlicker" {
	Properties{
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5
		_OcclusionColor("Occlusion Color", Color) = (0,0,0,0.5)
		_FlickerColor("Flicker Color", Color) = (1,1,1,0.5)
		_Period("Period",float) = 0.4
		_BacklightBorder("Backlight Border", float) = 0.5
		_DotScale("Dot Scale", float) = 0.7
		_FalloffDot("Falloff Dot", float) = 0.2
		_BidirCutoff("Bidirectional Cutoff", float) = 0.1
		[MaterialToggle] _BidirectionalLight("Bidirectional Light", Float) = 0	//light from behind is counted
	}
	SubShader {
		Tags
		{
			"Queue" = "Transparent-100"
			"RenderType" = "TransparentCutout"
			"DisableBatching"="True"
		}
		LOD 200

		Cull Off
		ZWrite On	//???
		Blend SrcAlpha OneMinusSrcAlpha

		//Transparency breaks the logic of the silhouette thing			
			

		CGPROGRAM
		// Lambert lighting model, and enable shadows on all light types
		#pragma surface surf SimpleLambert addshadow fullforwardshadows alpha:fade
		//#pragma debug

		//note: lambert has some extra features this one is missing?
		//can't use skybox reflections as well as antilight since it uses the light alpha channel while the unity data structures don't have light alpha
		//but it doesn't seem that the lighting difference matters?
		//sprites don't get skybox reflections and the ambient light seems to work properly still

		float _BidirectionalLight;
		float _FalloffDot;
		float _DotScale;
		float _BidirCutoff;

		float _Period;
		half4 _FlickerColor;

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

			//redo this?
			/*
			if (frac(_Time.z / _Period) > 0.5) {
				c.a = min(c.a, _FlickerColor.a);
			}
			*/
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

			//get scale (only X but sprites should be uniformly scaled in x and y so this should be fine)
			float k = length(mul(unity_ObjectToWorld, float4(0,1,0,0)));

			//minor optimization: this is equivalent to above
			//float k = length(float3(unity_ObjectToWorld[0][0], unity_ObjectToWorld[1][0], unity_ObjectToWorld[2][0]));

			//sobel filter (outline)
			float d = _MainTex_TexelSize.xy * (1 / k) * _BacklightBorder;

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
			
			//mini optimization: comparing with 0 means that the square root and /4 are irrelevant
			float w = gx * gx + gy * gy; //sqrt(gx * gx + gy * gy) / 4;

			//use the gloss value incorrectly I guess
			if (w > _BidirCutoff) {
				o.Gloss = 1;
			}

			gx = clamp(gx,-1,1);
			gy = clamp(gy,-1,1);
			half3 mapnormal = normalize(half3(-gy, -gx, 1)); //UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
			//mapnormal = normalize(half3(gy, gx, 1));

			//???
			//w = 0 negates translation?
			//normalize negates scale?
			//skew doesn't exist?
			//so only rotation is left (?????)
			//half3 newnormal = normalize(mul(unity_ObjectToWorld, half4(mapnormal, 0)));

			o.Normal = mapnormal;

			o.Albedo = c.rgb;
			
			//debug to check my scale finder
			//o.Albedo = float3(k,1,0);
			
			//to see the normals
			//o.Albedo = newnormal * 0.5 + half3(0.5, 0.5, 0.5);
			
			//o.Albedo.g = 0;
			//o.Albedo.b = 0;
			o.Alpha = c.a;

			clip(o.Alpha - _Cutoff);

			if (frac(_Time.z / _Period) > 0.5) {
				o.Albedo *= _FlickerColor.rgb;
				o.Alpha = min(o.Alpha, _FlickerColor.a);
			}
		}
		ENDCG
	}

	FallBack "Diffuse"
}