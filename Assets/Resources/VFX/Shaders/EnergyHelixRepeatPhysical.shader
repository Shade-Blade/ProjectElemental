Shader "VFX/Effect_EnergyHelixRepeatPhysical"
{
    Properties
    {
        [HDR]
        _Emission ("Emission", Color) = (1,1,1,1)
        _Color ("Color", Color) = (1,1,1,1)
        [HDR]
        _EmissionB ("Emission B", Color) = (1,1,1,1)
        _ColorB ("Color B", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        //[PerRendererData]
        _Cutoff ("Cutoff", float) = 0.5
        //[PerRendererData]
        _CutoffB ("Cutoff B", float) = 0.5
        //[PerRendererData]
        _CutoffC ("Cutoff C", float) = 0.5
        _TimeScaleX ("Time Scale X", float) = 2
        _TimeScaleY ("Time Scale Y", float) = 2
        _NoiseFactor ("Noise Factor", float) = 1
        _YFactor("Y Factor", float) = 1
        _Slope("Slope", float) = 1
        _XBonus("XBonus", float) = 1
        _MaxPlus("Max Plus", float) = 1
        _MinPlus("Min Plus", float) = 1
        _XUVFactor("X UV Factor", float) = 1
        _YUVFactor("Y UV Factor", float) = 1
        _CutoffOffset("Cutoff Offset", float) = 0.5
    }
    SubShader
    {
		Tags
		{
			"Queue" = "Geometry"
			"RenderType" = "TransparentCutout"
		}
        //LOD 200

        Cull Off
        Lighting Off
        //ZTest Off
        //ZWrite Off

        //Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Unlit //alpha:fade//Unlit //fullforwardshadows

        half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
            half4 c;
	        c.rgb = s.Albedo;
            c.a = s.Alpha;
            return half4(0,0,0,c.a); //c;
        }

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;
        half4 _Emission;
        fixed4 _ColorB;
        half4 _EmissionB;

        half _NoiseFactor;
        half _YFactor;
        half _Slope;
        half _XBonus;

        half _Cutoff;
        half _CutoffB;
        half _CutoffC;
        half _TimeScaleX;
        half _TimeScaleY;

        half _MaxPlus;
        half _MinPlus;

        half _XUVFactor;
        half _YUVFactor;

        half _CutoffOffset;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            float2 uv = IN.uv_MainTex * float2(_XUVFactor, _YUVFactor);
            uv.x +=_Time.x * _TimeScaleX;
            uv.y += _Time.x * _TimeScaleY;
            fixed4 c = tex2D (_MainTex, uv);
            // Metallic and smoothness come from slider variables
            o.Alpha = 1;

            float factor = (c.r * _NoiseFactor + (IN.uv_MainTex.y + (IN.uv_MainTex.x + _XBonus) * _Slope) * _YFactor);
            float factorB = (c.r * _NoiseFactor + (IN.uv_MainTex.y) * _YFactor);

            //weird calculation
            //so basically
            //if factor < _Cutoff
            //add 1 until it's too big
            //if factor > _CutoffC
            //subtract 1 until it's too small

            if (factor < _Cutoff) {
                factor = factor + min(_MaxPlus, ceil(_Cutoff - factor));
            }

            if (factor > _CutoffC) {
                factor = factor - min(_MinPlus, ceil(factor - _CutoffC));
            }

            if (factorB < _Cutoff - _CutoffOffset) {
                discard;
            }
            if (factorB > _CutoffC + _CutoffOffset) {
                discard;
            }

            if (factor > _CutoffB) {
                o.Albedo = _ColorB;
                o.Emission = _EmissionB;
                o.Alpha = _ColorB.a;
            } else {
                o.Albedo = _Color;
                o.Emission = _Emission;
                o.Alpha = _Color.a;
            }

            if (factor > _CutoffC) {
                clip(_CutoffC - factor);
            }

            clip(factor - _Cutoff);
        }
        ENDCG
    }
    //FallBack "Unlit"
}
