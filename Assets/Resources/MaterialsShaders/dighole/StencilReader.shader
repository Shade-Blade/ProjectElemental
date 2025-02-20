Shader "Custom/StencilHole_Bottom" {
	Properties {
		_Color ("Tint", Color) = (0, 0, 0, 1)
		_MainTex ("Texture", 2D) = "white" {}

		_BrightnessLow("Brightness Low", float) = 0.22
        _BrightnessMid("Brightness Mid", float) = 0.53
        _BrightnessHigh("Brightness High", float) = 1
        _BrightnessCutoffA("Brightness Cutoff A", float) = 0.05
        _BrightnessCutoffB("Brightness Cutoff B", float) = 0.55

		[IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
	}
	SubShader {
		Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

		//use ztest off to see how the writer having a ztest will stop it from writing to the stencil buffer when it is occluded
		ZTest Greater
		Cull Off

        //stencil operation
		Stencil{
			Ref [_StencilRef]
			Comp Equal
		}

		CGPROGRAM

		#pragma surface surf ToonRamp fullforwardshadows addshadow
		#pragma target 3.0

        float _BrightnessLow;
        float _BrightnessMid;
        float _BrightnessHigh;
        float _BrightnessCutoffA;
        float _BrightnessCutoffB;

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
		fixed4 _Color;

        struct Input {
            float2 uv_MainTex : TEXCOORD0;
            float3 viewDir;
        };

		void surf (Input IN, inout SurfaceOutput o) {
            // main texture
            half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            o.Albedo = c.rgb;
		}
		ENDCG
	}
	FallBack "Standard"
}