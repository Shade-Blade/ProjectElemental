//toon v2
Shader "Toon/ToonV2" {
    Properties{
         [HDR]
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB)", 2D) = "white" {}
        _BrightnessLow("Brightness Low", float) = 0.22
        _BrightnessMid("Brightness Mid", float) = 0.53
        _BrightnessHigh("Brightness High", float) = 1
        _BrightnessCutoffA("Brightness Cutoff A", float) = 0.05
        _BrightnessCutoffB("Brightness Cutoff B", float) = 0.55
    }
    
    SubShader{
        Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 200
        Cull Off
        
        CGPROGRAM
        
        #pragma surface surf ToonRamp fullforwardshadows addshadow
        
        float _BrightnessLow;
        float _BrightnessMid;
        float _BrightnessHigh;
        float _BrightnessCutoffA;
        float _BrightnessCutoffB;

        // custom lighting function based
        // on angle between light direction and normal
        #pragma lighting ToonRamp exclude_path:prepass
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
 
        
        
        sampler2D _MainTex;
        float4 _Color;
       
        
        struct Input {
            float2 uv_MainTex : TEXCOORD0;
            float3 viewDir;
        };
        
        
        void surf(Input IN, inout SurfaceOutput o) {
            // main texture
            half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            o.Albedo = c.rgb;
        }
        ENDCG
        
    }
    
    //Fallback "Diffuse"
}