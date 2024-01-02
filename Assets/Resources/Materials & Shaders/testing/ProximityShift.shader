Shader "Custom/ProximityShift"
{
    Properties
    {
        _Color ("Color A", Color) = (1,1,1,1)
        _ColorB ("Color B", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MainTexB ("Alt Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _FadeLength ("Fade Length", float) = 0.0
        _Distance ("Distance", float) = 1
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
        sampler2D _MainTexB;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _ColorB;

        float _FadeLength;
        float _Distance;
        
		uniform float4 _PlayerPos;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c;
            fixed4 ca = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            fixed4 cb = tex2D (_MainTexB, IN.uv_MainTex) * _ColorB;

            float delta = length(IN.worldPos - _PlayerPos.xyz);

            //Which texture to use?
            float interp;

            if (_FadeLength == 0) {
                delta = (delta - _Distance) > 0 ? 1 : -1;
            } else {
                delta = (delta - _Distance) / _FadeLength;
            }

            //[-1, 1] -> [0, 1]
            delta = saturate((delta + 1) / 2);
            interp = delta;

            c = lerp(ca, cb, interp);

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
