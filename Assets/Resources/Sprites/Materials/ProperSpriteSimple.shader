// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//= SilhouetteIncluded
//Use for basic sprites

Shader "Custom/ProperSpriteSimple" {
	Properties{
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5
		_OcclusionColor("Occlusion Color", Color) = (0,0,0,0.5)
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
		#pragma surface surf SimpleLambert addshadow fullforwardshadows
		//#pragma debug


		half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
			half3 realNormal = s.Normal;

			//rotation matrix
			//half angle = _AngleOffset;
			//half3x3 rotMatrix = half3x3(half3(cos(angle), sin(angle), 0), half3(-sin(angle), cos(angle), 0), half3(0,0,1));
			//realNormal = mul(rotMatrix, realNormal);

			half3 altLightDir = lightDir;

			half NdotL = dot (realNormal, altLightDir);	

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

			o.Albedo = c.rgb;
			
			o.Alpha = c.a;
			clip(o.Alpha - _Cutoff);
		}
		ENDCG
	}

	FallBack "Diffuse"
}