Shader "VertexDistortion/Resize" {
	//resize world relative to player
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		_ResizeScaleXZ("Resize Scale XZ", float) = 1
		_ResizeScaleY("Resize Scale Y", float) = 1
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

		float _ResizeScaleXZ;
		float _ResizeScaleY;

		float3 Warp(float3 v)
		{
			//using floats here to avoid half imprecision?
			//may not actually matter that much

			float3 wpos = mul(unity_ObjectToWorld, float4(v,1)).xyz;
			float3 diff = (wpos - _PlayerPos.xyz);

			return mul(unity_WorldToObject, float4(_PlayerPos.xyz + float3(diff.x * _ResizeScaleXZ, diff.y * _ResizeScaleY, diff.z * _ResizeScaleXZ),1));
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