Shader "Custom/FakeVolumetricLightRay" {    //make this cone shaped
    //show values to edit in inspector
    Properties {
        //_MainTex ("Texture", 2D) = "white" {}
        _AlphaMult ("Alpha Mult", float) = 0.5
        [HDR] _Emission ("Emission", color) = (0,0,0)
        [HDR] _FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
        _FresnelExponent ("Fresnel Exponent", float) = 1
    }
    SubShader {
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "RenderType"="Transparent" "Queue"="Transparent"}
        
        Blend SrcAlpha OneMinusSrcAlpha
        CGPROGRAM
        
        float _AlphaMult;
        //because all surface shaders need a lighting model
        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
    	    fixed4 c;
    	    c.rgb = s.Albedo; 
    	    c.a = s.Alpha;
            return half4(0,0,0,s.Alpha); //c;
        }

        //the shader is a surface shader, meaning that it will be extended by unity in the background to have fancy lighting and other features
        //our surface shader function is called surf and we use the standard lighting model, which means PBR lighting
        //fullforwardshadows makes sure unity adds the shadow passes the shader might need
        #pragma surface surf NoLighting alpha:fade
        #pragma target 3.0

        //sampler2D _MainTex;


        half _Smoothness;
        half _Metallic;
        half3 _Emission;

        float3 _FresnelColor;
        float _FresnelExponent;

        //input struct which is automatically filled by unity
        struct Input {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
            INTERNAL_DATA
        };

        //the surface shader function which sets parameters the lighting function then uses
        void surf (Input i, inout SurfaceOutput o) {
            //sample and tint albedo texture
            fixed4 col = fixed4(0,0,0,1); //tex2D(_MainTex, i.uv_MainTex);
            //col *= _Color;
            o.Albedo = col.rgb;


            //get the dot product between the normal and the view direction
            float fresnel = dot(i.worldNormal, i.viewDir);
            //invert the fresnel so the big values are on the outside
            fresnel = saturate(fresnel);
            //raise the fresnel value to the exponents power to be able to adjust it
            fresnel = pow(fresnel, _FresnelExponent);
            o.Alpha = fresnel * _AlphaMult;
            //combine the fresnel value with a color
            float3 fresnelColor = fresnel * _FresnelColor;
            //apply the fresnel value to the emission
            o.Emission = _Emission + fresnelColor;
        }
        ENDCG
    }
    FallBack "Standard"
}