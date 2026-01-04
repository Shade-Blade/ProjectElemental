//toon v2
Shader "Toon/ToonV2GlowTriplanar" {
    Properties{
         [HDR]
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB)", 2D) = "white" {}
         [HDR]
        _GlowColor("Glow Color", Color) = (1,1,1,1)
        _GlowTex("Base Glow (RGB)", 2D) = "white" {}
        _BrightnessLow("Brightness Low", float) = 0.22
        _BrightnessMid("Brightness Mid", float) = 0.53
        _BrightnessHigh("Brightness High", float) = 1
        _BrightnessCutoffA("Brightness Cutoff A", float) = 0.05
        _BrightnessCutoffB("Brightness Cutoff B", float) = 0.55
        _Scale("Scale", float) = 0.5
        _BrightnessMult("Dark Mult", float) = 0.75
        _TriplanarA("Triplanar A", 2D) = "white" {}
        _TriplanarB("Triplanar B", 2D) = "white" {}
        _TriplanarC("Triplanar C", 2D) = "white" {}
        _TriplanarD("Triplanar D", 2D) = "white" {}
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

        sampler2D _GlowTex;
        float4 _GlowColor;
       
        
        float _BrightnessMult;        
        float _Scale;
        sampler2D _TriplanarA;
        sampler2D _TriplanarB;
        sampler2D _TriplanarC;
        sampler2D _TriplanarD;        
        float4 _TriplanarA_ST;
        float4 _TriplanarB_ST;
        float4 _TriplanarC_ST;
        float4 _TriplanarD_ST;    
        
        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldPos;
            float3 worldNormal;
        };
        
        
        void surf(Input IN, inout SurfaceOutput o) {
            // main texture
                        fixed4 col1 = 0; //tex2D(triplanar, IN.worldPos.yz * _Scale);
            fixed4 col2 = 0; //tex2D(triplanar, IN.worldPos.xz * _Scale);
            fixed4 col3 = 0; //tex2D(triplanar, IN.worldPos.xy * _Scale);
            if (IN.uv_MainTex.x < 0.25) {
                col1 = tex2D(_TriplanarA, TRANSFORM_TEX((IN.worldPos.zy * _Scale), _TriplanarA));
                col2 = tex2D(_TriplanarA, TRANSFORM_TEX((IN.worldPos.xz * _Scale), _TriplanarA));
                col3 = tex2D(_TriplanarA, TRANSFORM_TEX((IN.worldPos.xy * _Scale), _TriplanarA));
            } else if (IN.uv_MainTex.x < 0.5) {
                col1 = tex2D(_TriplanarB, TRANSFORM_TEX((IN.worldPos.zy * _Scale), _TriplanarB));
                col2 = tex2D(_TriplanarB, TRANSFORM_TEX((IN.worldPos.xz * _Scale), _TriplanarB));
                col3 = tex2D(_TriplanarB, TRANSFORM_TEX((IN.worldPos.xy * _Scale), _TriplanarB));
            } else if (IN.uv_MainTex.x < 0.75) {
                col1 = tex2D(_TriplanarC, TRANSFORM_TEX((IN.worldPos.zy * _Scale), _TriplanarC));
                col2 = tex2D(_TriplanarC, TRANSFORM_TEX((IN.worldPos.xz * _Scale), _TriplanarC));
                col3 = tex2D(_TriplanarC, TRANSFORM_TEX((IN.worldPos.xy * _Scale), _TriplanarC));
            } else {
                col1 = tex2D(_TriplanarD, TRANSFORM_TEX((IN.worldPos.zy * _Scale), _TriplanarD));
                col2 = tex2D(_TriplanarD, TRANSFORM_TEX((IN.worldPos.xz * _Scale), _TriplanarD));
                col3 = tex2D(_TriplanarD, TRANSFORM_TEX((IN.worldPos.xy * _Scale), _TriplanarD));
            }

 
            float3 vec = abs(IN.worldNormal);

            fixed4 col;
            if (vec.x > vec.y && vec.x > vec.z) {
                col = col1;
            } else if (vec.y > vec.z) {
                col = col2;
            } else {
                col = col3;
            }
            
            float2 uv = IN.uv_MainTex;
            uv.y = (uv.y * lerp(_BrightnessMult, 1, col.r));
            //half4 c = tex2D(_MainTex, uv) * _Color;

            half4 c = tex2D(_MainTex, uv) * _Color;
            
            o.Albedo = c.rgb;

            half4 cB = tex2D(_GlowTex, uv) * _GlowColor;            
            o.Emission = cB.rgb;

        }
        ENDCG
        
    }
    
    //Fallback "Diffuse"
}