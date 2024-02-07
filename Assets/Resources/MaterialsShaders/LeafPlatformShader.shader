// xray mouse pos shader test v2.0 – mgear – http://unitycoder.com/blog

Shader "Custom/LeafPlatform"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
		_Radius("Hole Radius", Range(0.1,5)) = 0.5
		_BaseAlpha("Base Alpha", Range(0,1)) = 0.8
		_HoleAlpha("Hole Alpha", Range(0,1)) = 0.2

		//globals
		//_PlayerPos
		//_PlayerLight
	}
		SubShader
		{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
			LOD 100

			//Cull Off // draw backfaces also, comment this line if no need for backfaces

			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows alpha:fade


			struct Input
			{
				float2 uv_MainTex;
				float3 worldPos;
			};

			sampler2D _MainTex;
			uniform float _Radius;
			uniform float _BaseAlpha;
			uniform float _Cutoff;
			uniform float _HoleAlpha;

			uniform float4 _PlayerPos;
			uniform float _PlayerLeaf;

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				half3 col = tex2D(_MainTex, IN.uv_MainTex).rgb;
				float dx = length(_PlayerPos.x - IN.worldPos.x);
				float dy = length(_PlayerPos.y - IN.worldPos.y);
				float dz = length(_PlayerPos.z - IN.worldPos.z);
				float dist = 1;

				float value = 0; 

				value = 1 - 4 * _PlayerLeaf;
				value = clamp(value, 0, 1);

				dist = value + (dx * dx + dy * dy + dz * dz) * _Radius;

				dist = clamp(dist,0,1);

				o.Albedo = col; // color is from texture

				o.Alpha = (dist) * _BaseAlpha;  

				if (dist < _Cutoff) {
					o.Alpha = _HoleAlpha;	
				} else {
					o.Alpha = 1;
				}
			}

			/*
			void vert (inout appdata_full v) {
			}

			void frag (inout appdata_full v) {
			}
			*/
		
			ENDCG
		}
			FallBack "Diffuse"
}