Shader "Custom/FresnelGlowMinus" {
    //show values to edit in inspector
    Properties {
        [HDR]
        _Color ("Tint", Color) = (0, 0, 0, 1)
        [HDR]
        _MinusColor ("Minus Tint", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metalness", Range(0, 1)) = 0
        [HDR] _Emission ("Emission", color) = (0,0,0)
        [HDR] _MinusEmission ("Anti Emission", color) = (0,0,0)
    }
    SubShader {
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        CGPROGRAM

        //the shader is a surface shader, meaning that it will be extended by unity in the background to have fancy lighting and other features
        //our surface shader function is called surf and we use the standard lighting model, which means PBR lighting
        //fullforwardshadows makes sure unity adds the shadow passes the shader might need
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        half4 _Color;
        half4 _MinusColor;

        half _Smoothness;
        half _Metallic;
        half3 _Emission;
        half3 _MinusEmission;

        //input struct which is automatically filled by unity
        struct Input {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
            INTERNAL_DATA
        };

        //the surface shader function which sets parameters the lighting function then uses
        void surf (Input i, inout SurfaceOutputStandard o) {
            //sample and tint albedo texture
            fixed4 col = tex2D(_MainTex, i.uv_MainTex);
            col *= (_Color - _MinusColor);
            o.Albedo = col.rgb;

            //just apply the values for metalness and smoothness
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;

            o.Emission = _Emission - _MinusEmission;
        }
        ENDCG
    }
    FallBack "Standard"
}