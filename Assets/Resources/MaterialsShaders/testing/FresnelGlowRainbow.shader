Shader "Custom/FresnelGlowRainbow" {
    //show values to edit in inspector
    Properties {
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metalness", Range(0, 1)) = 0
        [HDR] _Emission ("Emission", color) = (0,0,0)

        _AngleDelta ("Angle Delta", Range(0,1)) = 0

        [HDR]
        _FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
        [PowerSlider(4)] _FresnelExponent ("Fresnel Exponent", Range(0.25, 4)) = 1
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
        fixed4 _Color;

        half _Smoothness;
        half _Metallic;
        half3 _Emission;

        half _AngleDelta;

        float3 _FresnelColor;
        float _FresnelExponent;

        //float4x4 UNITY_MATRIX_IV;

        //input struct which is automatically filled by unity
        struct Input {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
            INTERNAL_DATA
        };

        //color ramp thing
        fixed3 color_ramp(float w) {
            w -= floor(w);
            w *= 6;

            fixed3 output = fixed3(0,0,0);
            output[0] = abs(w - 3) - 1;
            output[1] = -abs(w - 2) + 2;
            output[2] = -abs(w - 4) + 2;
            output = saturate(output);

            return output;
        }

        //the surface shader function which sets parameters the lighting function then uses
        void surf (Input i, inout SurfaceOutputStandard o) {
            //sample and tint albedo texture
            fixed4 col = tex2D(_MainTex, i.uv_MainTex);
            col *= _Color;
            o.Albedo = col.rgb;

            //just apply the values for metalness and smoothness
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;

            //normals relative to the camera
            float3 diff = normalize(mul((float3x3)UNITY_MATRIX_V, -i.worldNormal));

            float3 relViewDir = normalize(mul((float3x3)UNITY_MATRIX_V, i.viewDir));
            diff = normalize(diff + relViewDir);
            //diff = relViewDir;

            //float3 vcross = cross(diff, normalize(i.viewDir));
            //diff = float3(-vcross.y, vcross.x, 0.0);

            //use atan2 to convert back to angle
            float angle = (atan2(diff.y, diff.x) / (2 * 3.1415926535)) + _AngleDelta;

            //get the dot product between the normal and the view direction
            float fresnel = dot(i.worldNormal, i.viewDir);
            //invert the fresnel so the big values are on the outside
            fresnel = saturate(1 - fresnel);
            //raise the fresnel value to the exponents power to be able to adjust it
            fresnel = pow(fresnel, _FresnelExponent);
            //combine the fresnel value with a color
            float3 fresnelColor = fresnel * color_ramp(angle) * _FresnelColor;
            //apply the fresnel value to the emission
            o.Emission = _Emission + fresnelColor;

            //sidenote: camera projection is messing things up (so it stops looking right if it isn't at the center of the viewing area)

            //o.Emission = half3(diff.x * 0.5 + 0.5, diff.y * 0.5 + 0.5, diff.z * 0.5 + 0.5);
            /*
            if (diff.z > 0.9) {
                o.Emission = half3(1,1,1);
            }
            */
        }
        ENDCG
    }
    FallBack "Standard"
}