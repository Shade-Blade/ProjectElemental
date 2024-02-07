Shader "Custom/Spikes" {
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert fullforwardshadows vertex:vert addshadow
		#pragma target 3.0

		struct Input {
			float4 vertColor;
		};

		void vert(inout appdata_full v, out Input o){
			float3 p = v.vertex.xyz;

			float timeOffset = 0.25 * sin(_Time.z * 2);

			float m = 0.625 * 6.28318530718;
			p.y = timeOffset * (sin(p.x * m) + sin(p.z * m));

			//normal is ortho to both tangents
			//note that these formulas are independent (x tangent does not involve z and vice versa)
			//x dir tangent = normalize(1, ???)
			//	??? = 0.625 * 6.28318530718 * cos(m * p.x)
			//z dir tangent = normalize(???, 1)
			//	??? = 0.625 * 6.28318530718 * cos(m * p.z)

			float3 xtangent = normalize(float3(1, timeOffset * m * cos(m * p.x), 0));
			float3 ztangent = normalize(float3(0, timeOffset * m * cos(m * p.z), 1));

			//order important (cross is not associative)
			float3 normal = normalize(cross(ztangent, xtangent));

			v.vertex.xyz = p;
			v.normal = normal;
			o.vertColor = v.color;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.vertColor.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}