Shader "Custom/CustomGradient"
{
    Properties
    {
        _ColorA ("Color A", Color) = (1,1,1,1)
        _ColorB ("Color B", Color) = (1,1,1,1)
        _ColorC ("Color C", Color) = (1,1,1,1)
        _ColorD ("Color D", Color) = (1,1,1,1)
        _ColorE ("Color E", Color) = (1,1,1,1)
        _ColorF ("Color F", Color) = (1,1,1,1)
        _ColorG ("Color G", Color) = (1,1,1,1)
        _ColorH ("Color H", Color) = (1,1,1,1)
        _ColorMult("Color Mult", Color) = (1,1,1,1)

        _BrightnessLow("Brightness Low", float) = 0.22
        _BrightnessMid("Brightness Mid", float) = 0.53
        _BrightnessHigh("Brightness High", float) = 1
        _BrightnessCutoffA("Brightness Cutoff A", float) = 0.05
        _BrightnessCutoffB("Brightness Cutoff B", float) = 0.55
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf ToonRamp fullforwardshadows addshadow


        float _BrightnessLow;
        float _BrightnessMid;
        float _BrightnessHigh;
        float _BrightnessCutoffA;
        float _BrightnessCutoffB;

        inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
        {
            #ifndef USING_DIRECTIONAL_LIGHT
                lightDir = normalize(lightDir);
            #endif
            float d = dot(s.Normal, lightDir) ;
            float lightIntensity = d; //? factor


            if (lightIntensity < _BrightnessCutoffA) {
                lightIntensity = 0;
            } else if (lightIntensity < _BrightnessCutoffB) {
                lightIntensity = _BrightnessMid;
            } else {
                lightIntensity = _BrightnessHigh;
            }
          
            //sus
            if (_LightColor0.a == 0) {
                atten = -atten;
            }
            
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * lightIntensity * atten;
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
