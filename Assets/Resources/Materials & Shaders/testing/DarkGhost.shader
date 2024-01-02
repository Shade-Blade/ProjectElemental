Shader "Custom/DarkGhost"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _AlphaMult ("AlphaMult", float) = 1.5
        _CMult ("Other Mult", float) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha // standard alpha blending

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf SimpleLambert fullforwardshadows alpha:fade

        float _AlphaMult;
        float _CMult;

        half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
			half NdotL = dot (s.Normal, lightDir);
            half4 c;

            if (_LightColor0.a == 0) {
                atten = -atten;
            }

			half k = step(0, NdotL);
            half r = k * atten * NdotL * _LightColor0.rgb;
	        c.rgb = s.Albedo * r;
            c.a = max((1 - s.Alpha * r * _CMult) * _AlphaMult, 0);
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
            o.Alpha = c.a * 0.5;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
