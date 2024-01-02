Shader "Custom/Triplanar" {
    Properties {
        _Texture1 ("Texture 1", 2D) = "white" {}
        _Texture2 ("Texture 2", 2D) = "white" {}
        _Texture3 ("Texture 3", 2D) = "white" {}
        _Scale ("Scale", float) = 0.1
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metalness", Range(0, 1)) = 0
        [HDR] _Emission ("Emission", color) = (0,0,0)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
       
        CGPROGRAM
        #pragma surface surf Standard
 
        sampler2D _Texture1;
        sampler2D _Texture2;
        sampler2D _Texture3;
        float _Scale;
 
        half _Smoothness;
        half _Metallic;
        half3 _Emission;

        struct Input {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
        };
 
        void surf (Input IN, inout SurfaceOutputStandard o) {
           
            fixed4 col1 = tex2D(_Texture2, IN.worldPos.yz * _Scale);
            fixed4 col2 = tex2D(_Texture1, IN.worldPos.xz * _Scale);
            fixed4 col3 = tex2D(_Texture3, IN.worldPos.xy * _Scale);
 
            float3 vec = abs(IN.worldNormal);

            fixed4 col;
            if (vec.x > vec.y && vec.x > vec.z) {
                col = col1;
            } else if (vec.y > vec.z) {
                col = col2;
            } else {
                col = col3;
            }
            //fixed4 col = //vec.x * col1 + vec.y * col2 + vec.z * col3;
 
            o.Albedo = col;
            o.Emission = _Emission;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
        }
 
        ENDCG
    }
    FallBack "Diffuse"
}