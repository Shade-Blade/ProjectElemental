Shader "Custom/InverseLight"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SubsurfaceColor ("Subsurface color", Color) = (1,1,1,1)
        _Thickness ("Thickness", float) = 1
        _Scale ("Scale", float) = 1
        _Distortion ("Distortion", float) = 1
        _Power ("Power", float) = 1
        _Attenuation ("Attenuation", float) = 1
        _Ambient ("Ambient", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf SimpleLambert //fullforwardshadows

        fixed4 _SubsurfaceColor;

        float _Thickness;
        float _Scale;
        float _Distortion;
        float _Power;
        float _Attenuation;
        float _Ambient;

        half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
			half NdotL = dot (s.Normal, lightDir);
            half4 c;

            if (_LightColor0.a == 0) {
                atten = -atten;
            }

            //add
            float3 H = normalize(lightDir + s.Normal * _Distortion);
            float VdotH = pow(saturate(dot(viewDir, -H)), _Power) * _Scale;
            float3 I = _Attenuation * (VdotH + _Ambient) * _Thickness;

			half k = max(NdotL, 0);
	        c.rgb = s.Albedo * k * atten * NdotL * _LightColor0.rgb + _LightColor0.rgb * I;
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
