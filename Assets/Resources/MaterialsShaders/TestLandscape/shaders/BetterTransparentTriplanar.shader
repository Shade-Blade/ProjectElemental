//https://github.com/ewersp/Shaders/blob/master/BetterTransparentDiffuse.shader
// An improvement to the default Unity "Transparent/Diffuse" shader to prevent see-through artifacts.
Shader "Toon/ToonBetterTransparentTriplanar" {
	Properties {
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" { }

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

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="False" "RenderType"="Transparent" }

		Cull Off	//doesn't seem to change anything, front face will block out back face so you can't see the backfaces from the outside of a closed surface

		// First Pass: Only render alpha (A) channel
		Pass {
			ColorMask A
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color;

			float4 vert(float4 vertex:POSITION) : SV_POSITION {
				return UnityObjectToClipPos(vertex);
			}

			fixed4 frag() : SV_Target {
				return _Color;
			}

			ENDCG
		}

		// Second Pass: Now render color (RGB) channel
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		
		
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
		#pragma surface surf ToonRamp alpha

		sampler2D _MainTex;
		fixed4 _Color;

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
        

		struct Input {
			float4 color:COLOR;
			float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
		};

		void surf(Input IN, inout SurfaceOutput o) {
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
			fixed4 c = tex2D(_MainTex, uv) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}

	Fallback "Legacy Shaders/Transparent/Diffuse"
}