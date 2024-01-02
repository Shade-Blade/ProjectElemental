Shader "Custom/IridescentTest"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _EdgeFactor ("EdgeFactor", float) = 1
        _ColorFactor ("ColorFactor", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
        };

        float _EdgeFactor;
        float _ColorFactor;

        // GPU Gems
        inline fixed3 floatclamp (fixed3 x)
        {
            float3 y = 1 - x * x;
            y = max(y, 0);
            return y;
        }
        fixed3 color_ramp (float w)
        {
            fixed x = saturate(w);
    
            return floatclamp
            (    fixed3
                (
                    4 * (x - 0.75),    // Red
                    4 * (x - 0.5),    // Green
                    4 * (x - 0.25)    // Blue
                )
            );
        }

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            fixed edgeFactor = abs(dot(IN.viewDir, IN.worldNormal));

            fixed edgeInterp = pow(edgeFactor, _EdgeFactor);

            //note: 1 - edgeFactor to make red be on the edge (more physically accurate)
            c = edgeInterp * c + (1 - edgeInterp) * fixed4(color_ramp(1 - pow(edgeFactor, _ColorFactor)), 1);

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
