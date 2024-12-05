//https://github.com/ewersp/Shaders/blob/master/BetterTransparentDiffuse.shader
// An improvement to the default Unity "Transparent/Diffuse" shader to prevent see-through artifacts.
Shader "Custom/BetterTransparent" {
	Properties {
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" { }
        _SpecColor ("Specular Color", Color) = (0,0,0,0)
        _SpecPower ("Specular Power", float) = 48.0
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
		
		
		float _SpecPower;

        half4 LightingSimpleSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
            half3 h = normalize (lightDir + viewDir);

            half diff = max (0, dot (s.Normal, lightDir));

            float nh = max (0, dot (s.Normal, h));
            float spec = pow (nh, _SpecPower);

            //Without this line there may be glitchy bright spots (bloom makes them very obvious and distracting)
            //(not always necessary, bright spots don't always appear for all models)
            spec = clamp(spec, 0, 1);

            if (_LightColor0.a == 0) {
                atten = -atten;
            }

            half4 c;
            c.rgb = atten * _LightColor0.rgb * (s.Albedo * diff + _SpecColor * spec);
            c.a = s.Alpha;
            return c;
        }
		#pragma surface surf SimpleSpecular alpha

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input {
			float4 color:COLOR;
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}

	Fallback "Legacy Shaders/Transparent/Diffuse"
}