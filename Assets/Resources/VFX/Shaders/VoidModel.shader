Shader "VFX/VoidModel"
{
    Properties
    {
        [HDR]
        //_Emission ("Emission", Color) = (1,1,1,1)
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture (Alpha only)", 2D) = "white" {}
        _TimeScaleA ("Time Scale", float) = 2
        _ScreenTex ("Screen Texture", 2D) = "white" {}
        _LowEnd ("Low end", float) = -5
        _HighEnd ("High end", float) = 0.4
        _Threshold ("Threshold", float) = 0.5
    }
    SubShader
    {
		Tags
		{
			"Queue" = "Geometry"
			"RenderType" = "TransparentCutout"
		}
        LOD 200

        Cull Off
        Lighting Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Unlit //fullforwardshadows

        half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
            half4 c;
	        c.rgb = s.Albedo;
            c.a = s.Alpha;
            return half4(0,0,0,1); //c;
        }

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _ScreenTex;
        float4 _ScreenTex_ST;

        struct Input
        {
            float4 screenPos;
            float2 uv_MainTex;
        };

        half _TimeScaleA;

        half _LowEnd;
        half _HighEnd;
        half _Threshold;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            float2 uv = IN.uv_MainTex;
            fixed4 c = tex2D (_MainTex, uv);
            o.Albedo = float3(0,0,0);
            o.Alpha = c.a;

            o.Emission = float4(0,0,0,0);
            float2 textureCoordinate = IN.screenPos.xy / IN.screenPos.w;
            float aspect = _ScreenParams.x / _ScreenParams.y;
            textureCoordinate.x = textureCoordinate.x * aspect;
            textureCoordinate = TRANSFORM_TEX(textureCoordinate, _ScreenTex);

            //precompute some numbers
            float a1 = sin(_Time.x * _TimeScaleA);
            float a2 = sin(_Time.x * _TimeScaleA * 2.21);
            float a3 = sin(_Time.x * _TimeScaleA * 4.26);
            float a4 = sin(_Time.x * _TimeScaleA * 0.61);
            float a5 = cos(_Time.x * _TimeScaleA);
            float a6 = cos(_Time.x * _TimeScaleA * 2.62);
            float a7 = cos(_Time.x * _TimeScaleA * 4.12);
            float a8 = cos(_Time.x * _TimeScaleA * 0.57);


            //scroll RGB independently
            float2 texScrollR = textureCoordinate;
            float2 texScrollG = textureCoordinate;
            float2 texScrollB = textureCoordinate;

            float adiff = a3 * 5.1 + a2 * 1.1 + a1 * 0.52;
            float bdiff = a7 * 4.1 + a6 * 1.3 + a5 * 0.61;
            float cdiff = a2 * 4.1 + a6 * 1.3 + a5 * 0.61;
            float ddiff = a7 * 4.1 + a5 * 1.3 + a1 * 0.61;

            texScrollR.x += adiff + bdiff;
            texScrollR.y += bdiff + cdiff;
            texScrollG.x += cdiff + ddiff;
            texScrollG.y += ddiff + adiff;
            texScrollB.x += adiff + cdiff;
            texScrollB.y += bdiff + ddiff;

            //note: make the rgb channels independent for more chaotic scrolling?
            fixed4 colR = tex2D(_ScreenTex, texScrollR);
            fixed4 colG = tex2D(_ScreenTex, texScrollG);
            fixed4 colB = tex2D(_ScreenTex, texScrollB);

            float3 vec = float3(colR.r, colG.g, colB.b);
            vec = normalize(vec);

            //point one color forward the others back
            float ar = abs(vec.r);
            float ag = abs(vec.g);
            float ab = abs(vec.b);
            float am = max(ar, max(ag, ab));

			//new version I guess
			float count = 0;
			if (ar > _Threshold) {
				count += 1;
			}
			if (ag > _Threshold) {
				count += 1;
			}
			if (ab > _Threshold) {
				count += 1;
			}

			count = count / 3;

			vec.r = _LowEnd + (_HighEnd - _LowEnd) * count;
			vec.g = _LowEnd + (_HighEnd - _LowEnd) * count;
			vec.b = _LowEnd + (_HighEnd - _LowEnd) * count;

            o.Albedo = vec;
            o.Alpha = 1;
        }
        ENDCG
    }
    //FallBack "Unlit"
}
