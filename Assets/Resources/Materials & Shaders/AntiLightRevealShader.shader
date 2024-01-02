// xray mouse pos shader test v2.0 – mgear – http://unitycoder.com/blog

Shader "Custom/AntiLightRevealMaterial"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_NoiseTex("Noise (only uses red channel)", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
		_Radius("Hole Radius", float) = 0.5
		_BaseAlpha("Base Alpha", Range(0,1)) = 0.8
		_OuterAlpha("Outer Alpha", Range(0,1)) = 0.2
		_NoisePower("Noise Power", Float) = 0.1
		_DistanceMove("Distance Move", Float) = 0.1
		_EdgeLight("Edge Light", Float) = 0.1
        _SpecColor ("Specular Color", Color) = (0,0,0,0)

		[HDR]_EmissionMult("Mult", Color) = (0,0,0,0)

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
			#pragma surface surf SimpleSpecular fullforwardshadows alpha:fade

			//half4 _SpecColor;

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
			uniform float _Cutoff;
			uniform float _OuterAlpha;

			uniform float4 _PlayerPos;
			uniform float _PlayerLight;
			uniform float _NoisePower;
			float _EdgeLight;

			uniform float4 _EmissionMult;
			float _DistanceMove;

			void surf(Input IN, inout SurfaceOutput o)
			{
				half3 col = tex2D(_MainTex, IN.uv_MainTex).rgb;
				float3 diff = IN.worldPos - _PlayerPos;
				float dx = length(diff.x);
				float dy = length(diff.y);
				float dz = length(diff.z);
				float dist = 1;

				float lightValue = 0; 

				lightValue = 1 - 4 * -_PlayerLight;
				lightValue = clamp(lightValue, 0, 1);

				dist = lightValue + (dx * dx + dy * dy + dz * dz) * (1 / _Radius);


				diff = normalize(diff);

				//hmm, maybe some poorly implemented triplanar noise may work here
				float2 xzUV = float2(diff.z + diff.x, diff.x - diff.z);
				float2 xyUV = float2(diff.x + diff.y, diff.y - diff.x);
				float2 yzUV = float2(diff.y + diff.z, diff.z - diff.y);

				xyUV.x += 0.3;
				xyUV.y += 0.7;
				yzUV.x += 0.7;
				yzUV.y += 0.3;

				xzUV.x -= sin(_Time.w * 0.06);
				xzUV.y -= cos(_Time.w * 0.065);
				xyUV.x -= cos(_Time.w * 0.04);
				xyUV.y -= sin(_Time.w * 0.035);
				yzUV.x -= sin(_Time.w * 0.055);
				yzUV.y -= cos(_Time.w * 0.045);

				xzUV *= 0.3;
				xyUV *= 0.3;
				yzUV *= 0.3;

				half noise = (tex2D(_NoiseTex, xzUV).r + tex2D(_NoiseTex, xyUV).g + tex2D(_NoiseTex, yzUV).b) / 3;
				noise = 0.5 - noise;

				dist += _DistanceMove * sin(3 * _Time.y);

				dist += noise * _NoisePower;
				dist = clamp(dist,0,1);

				o.Albedo = col; // color is from texture

				o.Alpha = (1 - dist) * _BaseAlpha;  

				if (dist > _Cutoff) {
					o.Alpha = _OuterAlpha;	
				} else {
					float modifiedDist = dist / _Cutoff;
					modifiedDist = (1) - modifiedDist * (1 - _EdgeLight);
					o.Emission = _EmissionMult * pow(modifiedDist,2);
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