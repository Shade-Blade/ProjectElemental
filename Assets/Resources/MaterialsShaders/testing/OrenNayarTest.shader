Shader "Custom/OrenNayarLighting"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Roughness ("Roughness", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf OrenNayar fullforwardshadows

        float _Roughness;
		inline float4 LightingOrenNayar(SurfaceOutput s,half3 lightDir,half3 viewDir,half atten){
		    //roughness A and B
		    float roughness = _Roughness;
		    float roughness2=roughness*roughness;
		    float2 oren_nayar_fraction = roughness2/(roughness2 + float2(0.33,0.09));
	        float2 oren_nayar = float2(1, 0) + float2(-0.5, 0.45) * oren_nayar_fraction;
	    
	        //Theta and phi
	        float2 cos_theta = saturate(float2(dot(s.Normal,lightDir),dot(s.Normal,viewDir)));
	        float2 cos_theta2 = cos_theta * cos_theta;
	        float sin_theta = sqrt((1-cos_theta2.x)*(1-cos_theta2.y));
	        float3 light_plane = normalize(lightDir - cos_theta.x*s.Normal);
	        float3 view_plane = normalize(viewDir - cos_theta.y*s.Normal);
	        float cos_phi = saturate(dot(light_plane, view_plane));
	    
	        //composition
	    
	        float diffuse_oren_nayar = cos_phi * sin_theta / max(cos_theta.x, cos_theta.y);
	    
	        float diffuse = cos_theta.x * (oren_nayar.x + oren_nayar.y * diffuse_oren_nayar);
		    float4 col;
		    col.rgb =s.Albedo * _LightColor0.rgb*(diffuse*atten);
		    col.a = s.Alpha;
		    return col;
		
		}

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
