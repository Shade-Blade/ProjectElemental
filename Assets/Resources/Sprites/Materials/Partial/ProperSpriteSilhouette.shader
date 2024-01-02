Shader "Custom/ProperSpriteSilhouette" {
	Properties{
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5
		_OcclusionColor("Occlusion Color", Color) = (0,0,1,1)

	}
		SubShader {
			LOD 200

			//Cull Off
			//ZWrite On
			//Blend One OneMinusSrcAlpha

			/*
			Pass {
				Cull Off
				ZWrite On
				Blend One OneMinusSrcAlpha
			}
			*/
			
			Tags
			{
				"Queue" = "Transparent-100"
				"RenderType" = "Transparent"
			}
			ZWrite Off
			ZTest Greater
			Blend SrcAlpha OneMinusSrcAlpha
			//Blend One OneMinusSrcAlpha
			//Color [_OcclusionColor]
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
			#pragma surface surf Lambert alpha
			//#pragma debug

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;
			fixed _Cutoff;

			struct Input
			{
				float2 uv_MainTex;
				float4 color : COLOR;
			};

			fixed4 _OcclusionColor;

			void surf(Input IN, inout SurfaceOutput o) {
				fixed4 c = _OcclusionColor; //tex2D(_MainTex, IN.uv_MainTex) * IN.color;
				o.Albedo = c.rgb;
				o.Alpha = c.a * tex2D(_MainTex, IN.uv_MainTex).a;
				clip(o.Alpha - _Cutoff);
			}
			ENDCG
		}

		FallBack "Diffuse"
}