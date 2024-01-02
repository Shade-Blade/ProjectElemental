// xray mouse pos shader test v2.0 – mgear – http://unitycoder.com/blog

Shader "Custom/Aether"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_NoiseTex("Noise (only uses red channel)", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
		_WeakCutoff("Base alpha cutoff", Range(0,1)) = 0.5
		_Radius("Hole Radius", float) = 0.5
		_BaseAlpha("Base Alpha", Range(0,1)) = 0.8
		_HoleAlpha("Hole Alpha", Range(0,1)) = 0.2
		_RimEdge("Rim Edge", Range(0, 1)) = 0.1
		_RimEdgeStep("Rim Edge Step", Range(0, 1)) = 0.1
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_NoisePower("Noise Power", Float) = 0.1
		_EmissionMult ("Emission Mult", Float) = 3
		_EmissionHoleMult ("Hole Emission Mult", Float) = 0.25
        _SpecColor ("Specular Color", Color) = (0,0,0,0)

		//globals
		//_PlayerPos
		//_PlayerAether
	}
		SubShader
		{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
			LOD 100

			//Cull Off // draw backfaces also, comment this line if no need for backfaces

			CGPROGRAM
			#pragma surface surf SimpleSpecular fullforwardshadows alpha:fade

			half4 LightingSimpleSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
				half3 h = normalize (lightDir + viewDir);

				half diff = max (0, dot (s.Normal, lightDir));

				float nh = max (0, dot (s.Normal, h));
				float spec = pow (nh, 48.0);

				if (_LightColor0.a == 0) {
					atten = -atten;
				}

				half4 c;
				c.rgb = (s.Albedo * _LightColor0.rgb * diff + _SpecColor * _LightColor0.rgb * spec) * atten;
				c.a = s.Alpha;
				return c;
			}


			struct Input
			{
				float2 uv_MainTex;
				float3 worldPos;
			};

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			uniform float _Radius;
			uniform float _BaseAlpha;
			uniform float _RimEdge;
			uniform float _RimEdgeStep;
			uniform float _Cutoff;
			uniform float _WeakCutoff;
			uniform float _HoleAlpha;
			uniform float _NoisePower;

			uniform half4 _RimColor;

			uniform float _EmissionMult;
			uniform float _EmissionHoleMult;

			uniform float4 _PlayerPos;
			uniform float _PlayerAether;

			void surf(Input IN, inout SurfaceOutput o)
			{
				half3 col = tex2D(_MainTex, IN.uv_MainTex).rgb;
				float dx = length(_PlayerPos.x - IN.worldPos.x);
				float dy = length(_PlayerPos.y - IN.worldPos.y);
				float dz = length(_PlayerPos.z - IN.worldPos.z);
				float dist = 1;

				float aetherValue = 0; 

				aetherValue = 1 - 4 * _PlayerAether;
				aetherValue = clamp(aetherValue, 0, 1);

				float weakdist = (dx * dx + dy * dy + dz * dz) * (1 / _Radius);

				float2 newUV = float2(IN.worldPos.x, IN.worldPos.y); //IN.uv_MainTex;

				float a = sin(_Time.w * 0.1);
				float b = cos(_Time.w * 0.13);
				float c = cos(_Time.w * 0.07);
				float d = sin(_Time.w * 0.09);
				float e = sin(IN.worldPos.x);
				float f = sin(IN.worldPos.z);

				newUV.x += a + IN.worldPos.z;
				newUV.y += b + f;

				float2 newUVb = float2(IN.worldPos.z, IN.worldPos.y); //IN.uv_MainTex;
				newUVb.x += 1.4 * c - IN.worldPos.x;
				newUVb.y += 1.4 * d - e;

				half noise = tex2D(_NoiseTex, newUV).r * 0.7 + tex2D(_NoiseTex, newUVb).r * 0.3;

				weakdist += noise * _NoisePower;
				dist = aetherValue + weakdist;
				dist = clamp(dist,0,1);
				weakdist = clamp(weakdist,0,1);

				noise = 0.5 - noise;

				//calculate rim level
				float rimedge = _RimEdge - dist + _BaseAlpha;
				rimedge = clamp(rimedge, 0, 1);

				rimedge = step(_RimEdgeStep, rimedge);

				//calculate rim color
				//oscillates between red, orange, yellow using _Time
				//(so 255 red, ? green, 0 blue)
				half3 rimcol;
				rimcol.r = 1;
				rimcol.g = sin(_Time.w * 0.5 + 16 * dist) * 0.125 + 0.125;
				rimcol.b = 0;

				o.Emission = _EmissionMult * lerp(half4(0,0,0,0), rimcol, rimedge);
				if (rimedge > 0) {
					col = _RimColor;
				}

				//o.Albedo = lerp(col, rimcol, rimedge); // color is from texture
				o.Albedo = col;

				dist = step(_Cutoff, dist);

				o.Alpha = dist * _BaseAlpha;  

				if (dist < _Cutoff) {
					o.Alpha = _HoleAlpha;	
					o.Emission *= _EmissionHoleMult;
				} else {
					if (weakdist < _WeakCutoff) {
						o.Emission = _EmissionMult * 0.05 * rimcol;
					}
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