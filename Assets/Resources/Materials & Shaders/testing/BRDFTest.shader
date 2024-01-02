Shader "Custom/BDRFOriginal"    //x = view, y = light
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BRDFTex ("BRDF (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf BRDF fullforwardshadows

        
        sampler2D _BRDFTex;

        //"bidirectional radiance distribution function" = brdf
        half4 LightingBRDF (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
			half NdotL = dot (s.Normal, lightDir);
            half NdotV = dot (s.Normal, viewDir);

            half diff = (NdotL * 0.5) + 0.5;            

            diff = saturate(diff);
            NdotV = saturate(NdotV);
            half2 brdfUV = half2(NdotV, diff);

            brdfUV *= 0.98;
            brdfUV += half2(0.01, 0.01);

            fixed3 rampOutput = tex2D(_BRDFTex, brdfUV.xy).rgb;

            half4 c;

	        c.rgb = s.Albedo * atten * _LightColor0.rgb * rampOutput;
            c.a = s.Alpha;
            return c;
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
