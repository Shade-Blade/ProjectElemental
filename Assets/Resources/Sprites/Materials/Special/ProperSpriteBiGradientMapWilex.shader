// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//= SilhouetteIncluded + AutoOutlineNormals
//So it has all the features I need for sprites in general

//Note: silhouette shows up if alpha is below 1 (normally the drawing of the sprite fully occludes the silhouette)

Shader "Custom/ProperSpriteBiGradientMapWilex" {
	Properties{
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5
		_OcclusionColor("Occlusion Color", Color) = (0,0,0,0.5)
		_BacklightBorder("Backlight Border", float) = 0.5
		_DotScale("Dot Scale", float) = 0.7
		_FalloffDot("Falloff Dot", float) = 0.2
		_BidirCutoff("Bidirectional Cutoff", float) = 0.1
		_RibbonCutoffA("Ribbon Cutoff A", float) = 0.75
		_RibbonCutoffB("Ribbon Cutoff B", float) = 0.85
		_WeaponCutoffA("Weapon Cutoff A", float) = 0.55
		_WeaponCutoffB("Weapon Cutoff B", float) = 0.7
		_WeaponCutoffC("Weapon Cutoff C", float) = 0.85
		[MaterialToggle] _BidirectionalLight("Bidirectional Light", Float) = 0	//light from behind is counted

		[HDR]
		_BlackColor("Black Color", Color) = (0,0,1,1)
		[HDR]
		_GrayColor("Gray Color", Color) = (0,0,1,1)
		[HDR]
		_WhiteColor("White Color", Color) = (0,0,1,1)
		_Midpoint("Midpoint", Range(0,1)) = 0.5
		_Leak("Leak", Range(0,1)) = 0.5
	}
	SubShader {
		Tags
		{
			"Queue" = "Geometry+1"
			"RenderType" = "TransparentCutout"
			"DisableBatching"="True"	//looking at the frame debugger this makes no difference? (But I want to be safe) (Note: currently the only reason for this to be here is because I need access to ObjectToWorld and batching will destroy that by creating a single mesh with an identity OTW matrix)
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
			

		
		Pass {
			ZWrite Off
			ZTest Always

			// First Pass: Silhouette
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			float4 _OcclusionColor;
			
			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _Cutoff;
			
			struct vertInput {
				float4 vertex:POSITION;
				float3 normal:NORMAL;
                float2 uv : TEXCOORD0;
			};
			
			struct fragInput {
				float4 pos:SV_POSITION;
                float2 uv : TEXCOORD0;
			};
			
			fragInput vert(vertInput i) {
				fragInput o;
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				o.pos = UnityObjectToClipPos(i.vertex);
				return o;
			}  
			
			float4 frag(fragInput i) : COLOR {
				fixed4 col = tex2D(_MainTex, i.uv);
				if (col.a < _Cutoff) {
					return float4(0,0,0,0);
				}
				return _OcclusionColor;
			}
			
			ENDCG
		}
			
			

		CGPROGRAM
		// Lambert lighting model, and enable shadows on all light types

		//shadows are broken for some reason with alpha fade
		//But I'm turning shadows off in general?
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

		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

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

		//color ramp thing
        fixed3 color_ramp(float w) {
            w -= floor(w);
            w *= 6;

            fixed3 output = fixed3(0,0,0);
            output[0] = abs(w - 3) - 1;
            output[1] = -abs(w - 2) + 2;
            output[2] = -abs(w - 4) + 2;
            output = saturate(output);

            return output;
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
		
		fixed _RibbonCutoffA;
		fixed _RibbonCutoffB;
		fixed _WeaponCutoffA;
		fixed _WeaponCutoffB;
		fixed _WeaponCutoffC;

		half4 _WRibbonColorA;
		half4 _WRibbonColorB;
		half4 _WRibbonColorC;

		fixed _WRibbonMimic;
		fixed _WRibbonRainbow;

		half4 _WWeaponColorA;
		half4 _WWeaponColorB;
		half4 _WWeaponColorC;
		half4 _WWeaponColorD;

		half4 _BlackColor;
		half4 _GrayColor;
		half4 _WhiteColor;

		float _Leak;
		float _Midpoint;

		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o) {

			half4 c = tex2D(_MainTex, IN.uv_MainTex);

			//calculate weapon mask stuff early so that blue ribbons don't trigger this
			half bl = c.b;
			half bdelta = c.b - max(c.r, c.g);

			//Ribbon mask
			half rbm = max(c.r, c.b);
			half rdelta = abs(c.r - c.b);

			if (rbm > 0.08 && rdelta < 0.08 && c.g < rbm / 2) {
				if (rbm < _RibbonCutoffA) {
					c = _WRibbonColorA;
				} else if (rbm < _RibbonCutoffB) {
					c = _WRibbonColorB;
				} else {
					c = _WRibbonColorC;
				}
			
				if (_WRibbonMimic == 1) {
					fixed timeDelta = 0.15 * (1 + sin(_Time.y));
					c = lerp(c, fixed4(0.71, 0.369, 0.965, 1), timeDelta);
				}
				if (_WRibbonRainbow == 1) {
					//reinterpret c
					//new color = (r channel * rainbow + g channel * gray)
					c = half4(c.r * color_ramp(_Time.x * 2 + 4 * IN.uv_MainTex.x) + c.g * half3(1,1,1), 1);
					//c *= fixed4(color_ramp(_Time.x * 2 + 4 * IN.uv_MainTex.x), 1);
				}
			}
			
			//weapon mask
			if (bl > 0.08 && bdelta > 0.08 && max(c.r, c.g) < bl / 2) {
				if (bl < _WeaponCutoffA) {
					c = _WWeaponColorA;
				} else if (bl < _WeaponCutoffB) {
					c = _WWeaponColorB;
				} else if (bl < _WeaponCutoffC) {
					c = _WWeaponColorC;
				} else {
					c = _WWeaponColorD;
				}
			}

			c *= IN.color;

			//get scale (only X but sprites should be uniformly scaled in x and y so this should be fine)
			//float k = length(mul(unity_ObjectToWorld, float4(1,0,0,0)));

			//minor optimization: this is equivalent to above
			float k = length(float4(unity_ObjectToWorld[0][0], unity_ObjectToWorld[1][0], unity_ObjectToWorld[2][0], unity_ObjectToWorld[3][0]));

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
			//so only rotation is left (?????)
			//half3 newnormal = normalize(mul(unity_ObjectToWorld, half4(mapnormal, 0)));

			o.Normal = mapnormal;


			fixed lerpCoeff = RGBToHSV(c.rgb).z;

			fixed4 gradColor;

			if (lerpCoeff > _Midpoint) {
				gradColor = lerp(_GrayColor,_WhiteColor,(lerpCoeff - _Midpoint)/(1 - _Midpoint));
			} else {
				gradColor = lerp(_BlackColor,_GrayColor,(lerpCoeff)/(_Midpoint));
			}

			fixed4 newcol = lerp(gradColor, c, _Leak);
			newcol.a = min(c.a, gradColor.a);

			o.Albedo = newcol;
			
			//debug to check my scale finder
			//o.Albedo = float3(k,1,0);
			
			//to see the normals
			//o.Albedo = newnormal * 0.5 + half3(0.5, 0.5, 0.5);
			
			//o.Albedo.g = 0;
			//o.Albedo.b = 0;
			o.Alpha = newcol.a;

			clip(c.a - _Cutoff);
		}
		ENDCG
	}

	FallBack "Diffuse"
}