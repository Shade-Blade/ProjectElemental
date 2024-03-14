Shader "VFX/EnergyCylinderTwoRainbow"
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
        _CutoffB ("Cutoff B", float) = 0.5
        _TimeScaleX ("Time Scale X", float) = 2
        _TimeScaleY ("Time Scale Y", float) = 2
        _TimeScaleZ ("Time Scale Z", float) = 2
        _NoiseFactor ("Noise Factor", float) = 1
        _YFactor("Y Factor", float) = 1
    }
    SubShader
    {
        //note: transparency has problems (z sorting messes up)
		Tags
		{
			"Queue" = "Geometry"
			"RenderType" = "Transparent"
		}
        LOD 200

        Cull Off
        Lighting Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Unlit//Unlit //fullforwardshadows

        half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
            half4 c;
	        c.rgb = s.Albedo;
            c.a = s.Alpha;
            return half4(0,0,0,1); //c;
        }

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        fixed4 _Color;
        half4 _Emission;
        fixed4 _ColorB;
        half4 _EmissionB;

        half _NoiseFactor;
        half _YFactor;

        half _Cutoff;
        half _CutoffB;
        half _TimeScaleX;
        half _TimeScaleY;
        half _TimeScaleZ;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

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

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            float2 uv = IN.uv_MainTex;
            uv.x +=_Time.x * _TimeScaleX;
            uv.y += _Time.x * _TimeScaleY;
            fixed4 c = tex2D (_MainTex, uv);
            // Metallic and smoothness come from slider variables
            o.Alpha = 1;

            float factor = (c.r * _NoiseFactor + IN.uv_MainTex.y * _YFactor);

            float3 localPos = IN.worldPos -  mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
            float angle = (atan2(localPos.z, localPos.x) / (2 * 3.1415926535)) + _Time.x * _TimeScaleZ;

            float3 colorMap = color_ramp(angle);

            if (factor > _CutoffB) {
                o.Albedo = _ColorB;
                o.Emission = colorMap * (_EmissionB.r - _EmissionB.g) + float3(1,1,1) * _EmissionB.g;
                clip(_ColorB.a - 0.01);
            } else {
                o.Albedo = _Color;
                o.Emission = colorMap * (_Emission.r - _Emission.g) + float3(1,1,1) * _Emission.g;
                clip(_Color.a - 0.01);
            }

            clip(factor - _Cutoff);
        }
        ENDCG
    }
    //FallBack "Unlit"
}
