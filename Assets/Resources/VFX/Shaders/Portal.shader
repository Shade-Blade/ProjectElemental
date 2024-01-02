Shader "VFX/Portal"
{
    Properties
    {
        [HDR]
        //_Emission ("Emission", Color) = (1,1,1,1)
        _Color ("Color", Color) = (1,1,1,1)
        [HDR]
        _ScreenColorA ("Screen Color A", Color) = (1,1,1,1)
        [HDR]
        _ScreenColorB ("Screen Color B", Color) = (1,1,1,1)
        [HDR]
        _ScreenColorC ("Screen Color C", Color) = (1,1,1,1)
        _Midpoint ("Midpoint", float) = 0.5
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        //[PerRendererData]
        _Cutoff ("Cutoff", float) = 0.5
        _TimeScaleX ("Time Scale X", float) = 2
        _TimeScaleY ("Time Scale Y", float) = 2
        _TimeScaleXS ("Time Scale X (Screen)", float) = 2
        _TimeScaleYS ("Time Scale Y (Screen)", float) = 2
        _NoiseFactor ("Noise Factor", float) = 1
        _YFactor("Y Factor", float) = 1
        _ScreenTex ("Texture", 2D) = "white" {}
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
        ZWrite Off  //so stuff behind shows up in front
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

        fixed4 _Color;
        float4 _ScreenColorA;
        float4 _ScreenColorB;
        float4 _ScreenColorC;
        fixed _Midpoint;
        //half4 _Emission;

        half _NoiseFactor;
        half _YFactor;

        half _Cutoff;
        half _TimeScaleX;
        half _TimeScaleY;
        half _TimeScaleXS;
        half _TimeScaleYS;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            float2 uv = IN.uv_MainTex;
            uv.x +=_Time.x * _TimeScaleX;
            uv.y += _Time.x * _TimeScaleY;
            fixed4 c = tex2D (_MainTex, uv);
            o.Albedo = _Color;
            // Metallic and smoothness come from slider variables
            o.Alpha = 1;


            half dist = (c.r * _NoiseFactor + IN.uv_MainTex.y * _YFactor);

            half3 rimcol;
			rimcol.r = 1;
			rimcol.g = sin(_Time.w * 0.5 + 16 * dist) * 0.125 + 0.125;
			rimcol.b = 0;

            o.Emission = rimcol * 3;

            if (dist - _Cutoff < 0) {
                o.Emission = float4(0,0,0,0);
                float2 textureCoordinate = IN.screenPos.xy / IN.screenPos.w;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                textureCoordinate.x = textureCoordinate.x * aspect;
                textureCoordinate = TRANSFORM_TEX(textureCoordinate, _ScreenTex);

                //Cool scrolling
                float2 texScrollA = textureCoordinate;
                float2 texScrollB = textureCoordinate;

                float xdiff = _Time.x * _TimeScaleXS;
                float ydiff = _Time.x * _TimeScaleYS;

                float delta = 0.25 * sin(_Time.x * 1.25) + 0.5;

                texScrollA.x += xdiff;
                texScrollB.x += xdiff * delta;
                texScrollA.y += ydiff * (1 - delta);
                texScrollB.y -= ydiff;

                fixed4 colA = tex2D(_ScreenTex, texScrollA);
                fixed4 colB = tex2D(_ScreenTex, texScrollB);

                fixed4 col = lerp(colA, colB, 0.25 * sin(_Time.x) + 0.5);

                fixed lerpCoeff = col.r;

                fixed4 gradColor;

                float4 whiteCol;
                if (_ScreenColorA.a == 0) {
                    whiteCol = _ScreenColorA * -1;
                } else {
                    whiteCol = _ScreenColorA;
                }
                float4 grayCol;
                if (_ScreenColorB.a == 0) {
                    grayCol = _ScreenColorB * -1;
                } else {
                    grayCol = _ScreenColorB;
                }
                float4 blackCol;
                if (_ScreenColorC.a == 0) {
                    blackCol = _ScreenColorC * -1;
                } else {
                    blackCol = _ScreenColorC;
                }

			    if (lerpCoeff > _Midpoint) {
				    gradColor = lerp(grayCol,whiteCol,(lerpCoeff - _Midpoint)/(1 - _Midpoint));
			    } else {
				    gradColor = lerp(blackCol,grayCol,(lerpCoeff)/(_Midpoint));
			    }

                o.Albedo = gradColor.rgb;
            }
        }
        ENDCG
    }
    //FallBack "Unlit"
}
