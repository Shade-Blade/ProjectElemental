Shader "Custom/IridescentTest"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _EdgeFactor ("EdgeFactor", float) = 1
        _ColorFactor ("ColorFactor", float) = 1

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

            spec = clamp(spec, 0, 1);

            half4 c;
            c.rgb = atten * _LightColor0.rgb * (s.Albedo * diff + _SpecColor * spec);
            c.a = s.Alpha;
            return c;
        }

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

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            fixed edgeFactor = abs(dot(IN.viewDir, IN.worldNormal));

            fixed edgeInterp = pow(edgeFactor, _EdgeFactor);

            //note: 1 - edgeFactor to make red be on the edge (more physically accurate)
            c = edgeInterp * c + (1 - edgeInterp) * fixed4(color_ramp(1 - pow(edgeFactor, _ColorFactor)), 1);

            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
