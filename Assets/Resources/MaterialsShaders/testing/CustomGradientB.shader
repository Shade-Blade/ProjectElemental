Shader "Custom/CustomGradientB"
{
    Properties
    {
        [HDR]
        _ColorA ("Color A", Color) = (1,1,1,1)
        [HDR]
        _ColorB ("Color B", Color) = (1,1,1,1)
        [HDR]
        _ColorC ("Color C", Color) = (1,1,1,1)
        [HDR]
        _ColorD ("Color D", Color) = (1,1,1,1)
        [HDR]
        _ColorE ("Color E", Color) = (1,1,1,1)
        [HDR]
        _ColorF ("Color F", Color) = (1,1,1,1)
        [HDR]
        _ColorG ("Color G", Color) = (1,1,1,1)
        [HDR]
        _ColorH ("Color H", Color) = (1,1,1,1)
        [HDR]
        _ColorMult("Color Mult", Color) = (1,1,1,1)
        [HDR]
        _SpecColor ("Specular Color", Color) = (0,0,0,0)
        _SpecPower ("Specular Power", float) = 48.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf SimpleSpecular fullforwardshadows

        //half4 _SpecColor;
        float _SpecPower;

        half4 LightingSimpleSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
            half3 h = normalize (lightDir + viewDir);

            half diff = max (0, dot (s.Normal, lightDir));

            float nh = max (0, dot (s.Normal, h));
            float spec = pow (nh, _SpecPower);

            if (_LightColor0.a == 0) {
                atten = -atten;
            }

            half4 c;
            c.rgb = atten * _LightColor0.rgb * (s.Albedo * diff + _SpecColor * spec);
            c.a = s.Alpha;
            return c;
        }

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0


        struct Input
        {
            float2 uv_MainTex;
        };

        half4 _ColorA;
        half4 _ColorB;
        half4 _ColorC;
        half4 _ColorD;
        half4 _ColorE;
        half4 _ColorF;
        half4 _ColorG;
        half4 _ColorH;

        half4 _ColorMult;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            half4 c;

            if (IN.uv_MainTex.x < 0.25) {
                c = lerp(_ColorA, _ColorB, 1 - IN.uv_MainTex.y);
            } else if (IN.uv_MainTex.x < 0.5) {
                c = lerp(_ColorC, _ColorD, 1 - IN.uv_MainTex.y);
            } else if (IN.uv_MainTex.x < 0.75) {
                c = lerp(_ColorE, _ColorF, 1 - IN.uv_MainTex.y);
            } else {
                c = lerp(_ColorG, _ColorH, 1 - IN.uv_MainTex.y);
            }

            c *= _ColorMult;

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
