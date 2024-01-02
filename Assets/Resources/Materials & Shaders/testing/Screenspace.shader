Shader "Custom/ScreenspaceTexture" {
    Properties {
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _ScreenTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metalness", Range(0, 1)) = 0
        _TextureStrength("Texture Strength", Range(0, 1)) = 1
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Emission ("Emission", color) = (0,0,0)
    }
    SubShader {
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _ScreenTex;
        sampler2D _MainTex;
        float4 _ScreenTex_ST;
        fixed4 _Color;

        half _Smoothness;
        half _Metallic;
        half3 _Emission;

        float _TextureStrength;

        struct Input {
            float4 screenPos;
            float2 uv_MainTex;
        };

        void surf (Input i, inout SurfaceOutputStandard o) {
            float2 textureCoordinate = i.screenPos.xy / i.screenPos.w;
            float aspect = _ScreenParams.x / _ScreenParams.y;
            textureCoordinate.x = textureCoordinate.x * aspect;
            textureCoordinate = TRANSFORM_TEX(textureCoordinate, _ScreenTex);

            fixed4 col = lerp(tex2D(_ScreenTex, textureCoordinate), tex2D (_MainTex, i.uv_MainTex), (1 - _TextureStrength));
            col *= _Color;
            o.Albedo = col.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Emission = _Emission;
        }
        ENDCG
    }
    FallBack "Standard"
}