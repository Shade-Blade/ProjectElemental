Shader "Custom/Sand"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SandTex ("Sand", 2D) = "white" {}
        //_DiffTex ("Shift", 2D) = "white" {}
        _SpecColor ("Specular Color", Color) = (0,0,0,0)
        _SpecPower ("Specular Power", float) = 48.0
        _SandPower ("Sand Power", float) = 48.0
        [HDR]
        _Emission ("Emission", Color) = (0,0,0,0)
        _GlitterPower ("Glitter Power", float) = 48.0
        _GlitterLerp ("Glitter Lerp", float) = 0.6
        _DiffWarp ("DiffWarp", float) = 0.1
        _DiffFrequency ("DiffFrequency", float) = 0.1
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

        sampler2D _MainTex;
        sampler2D _SandTex;
        //sampler2D _DiffTex;

        float _SandPower;

        half4 _Emission;
        float _GlitterPower;
        float _GlitterLerp;

        half _DiffWarp;
        half _DiffFrequency;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
            float3 worldNormal;
            INTERNAL_DATA
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
            float3 worldNormal = WorldNormalVector (IN, o.Normal);

            fixed3 diff;
            fixed3 sand;
            
            if (abs(worldNormal.y) > 0.5) {
                diff = tex2D (_SandTex, float2((IN.worldPos.x + IN.worldPos.z) * _DiffFrequency, (IN.worldPos.z - IN.worldPos.x) * _DiffFrequency)).rgb;
                sand = tex2D (_SandTex, float2(IN.worldPos.x, IN.worldPos.z)).rgb;
            } else {
                if (abs(worldNormal.x) > abs(worldNormal.z)) {
                    diff = tex2D (_SandTex, float2((IN.worldPos.y + IN.worldPos.z) * _DiffFrequency, (IN.worldPos.z - IN.worldPos.y) * _DiffFrequency)).rgb;
                    sand = tex2D (_SandTex, float2(IN.worldPos.y, IN.worldPos.z)).rgb;
                } else {
                    diff = tex2D (_SandTex, float2((IN.worldPos.x + IN.worldPos.y) * _DiffFrequency, (IN.worldPos.y - IN.worldPos.x) * _DiffFrequency)).rgb;
                    sand = tex2D (_SandTex, float2(IN.worldPos.x, IN.worldPos.y)).rgb;
                }
            }

            diff -= fixed3(0.5, 0.5, 0.5);
            diff *= _DiffWarp;

            float3 snormal = normalize(sand + diff); 

            //float y = IN.worldNormal.y;
            float3 otherNormal = o.Normal;
            otherNormal = normalize(lerp(o.Normal, snormal, _GlitterLerp));
            o.Normal = normalize(lerp(o.Normal, snormal, _SandPower));

            float emissionPower = pow (max (0, dot (otherNormal, IN.viewDir)), _GlitterPower);

            o.Emission = emissionPower * _Emission;

            o.Albedo = c.rgb; //worldNormal * 0.5 + float3(0.5, 0.5, 0.5); //c.rgb
            // Metallic and smoothness come from slider variables
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
