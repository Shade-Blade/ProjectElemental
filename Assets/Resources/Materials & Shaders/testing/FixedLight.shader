Shader "Custom/FixedLight"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf SimpleLambert fullforwardshadows

        half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
			half NdotL = dot (s.Normal, lightDir);
            half4 c;

            if (_LightColor0.a == 0) {
                atten = -atten;
            }

            //sussy conversion
            //no actually it uses the intensity multiplier :P

            fixed f = 0;

            //"0.25"
            if (_LightColor0.a > 0 && _LightColor0.a < 0.25) {
                atten = -step(0.1, atten);
                f = 1;
            }

            //"0.75"
            if (_LightColor0.a > 0.75 && _LightColor0.a < 1) {
                atten = step(0.1, atten);
                f = 1;
            }

			half k = step(0, NdotL);
            half n = max(f, NdotL);
            half u = max(f, k);
	        c.rgb = s.Albedo * u * n * atten * _LightColor0.rgb;
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
