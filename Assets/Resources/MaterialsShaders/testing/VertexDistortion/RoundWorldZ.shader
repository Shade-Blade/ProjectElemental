Shader "VertexDistortion/RoundWorldZ" {
	//Paraboloid distortion (though not completely, there is a range of no curvature)
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		_FlatThreshold("Flat Threshold", float) = 1
		_CurvatureStrength("Curvature Strength", float) = 1
    }
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert fullforwardshadows vertex:vert addshadow
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex;

		uniform float4 _PlayerPos;

		float _FlatThreshold;
		float _CurvatureStrength;

		float3 Warp(float3 v)
		{
			//note: floats probably better here, because halfs suffer from a lot more float imprecision a lot earlier
			//might not be that bad but eh

			float3 wpos = mul(unity_ObjectToWorld, float4(v,1)).xyz;
			half2 xzDist = (wpos.x - _PlayerPos.x);
			half dist = length(xzDist);

			dist = max(0, dist - _FlatThreshold);
			wpos.y -= dist * dist * _CurvatureStrength;
			wpos = mul(unity_WorldToObject, float4(wpos,1));

			return wpos;
		}

		void vert(inout appdata_full v){
			v.vertex.xyz = Warp(v.vertex.xyz);
		}

		void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}