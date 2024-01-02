Shader "Hidden/WorldPositionBloom"
{
    Properties
    {
        _MainTex ("-", 2D) = ""{}
        _BlurStrength("Blur Strength", float) = 1
        _BlurStrengthB("Blur Strength", float) = 2
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D_float _CameraDepthTexture;
    float4x4 _InverseView;

    half _BlurStrength;
    half _BlurStrengthB;

	uniform float4 _PlayerPos;

    half4 fragNeutral (v2f_img i) : SV_Target {        
        half4 source = tex2D(_MainTex, i.uv);
        source.a = 1;
        return source;
    }

    fixed4 frag (v2f_img i) : SV_Target
    {
        const float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
        const float2 p13_31 = float2(unity_CameraProjection._13, unity_CameraProjection._23);
        const float isOrtho = unity_OrthoParams.w;
        const float near = _ProjectionParams.y;
        const float far = _ProjectionParams.z;

        float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
        #if defined(UNITY_REVERSED_Z)
                d = 1 - d;
        #endif
        float zOrtho = lerp(near, far, d);
        float zPers = near * far / lerp(far, near, d);
        float vz = lerp(zPers, zOrtho, isOrtho);

        float3 vpos = float3((i.uv * 2 - 1 - p13_31) / p11_22 * lerp(vz, 1, isOrtho), -vz);

        _InverseView = unity_CameraToWorld;

        float4 cpos = float4(_WorldSpaceCameraPos, 0);

        //don't ask why, it just works this way
        //(real answer: camera space has inverted z axis for some reason?)
        vpos.z *= -1;

        float4 wpos = cpos + mul(_InverseView, float4(vpos, 0));

        half4 source = tex2D(_MainTex, i.uv);

        float bd = _BlurStrength;

        //8 samples
        half4 kernel;

        if (bd > 0) {
            //3x3 blur
            
            half4 a1 = tex2D(_MainTex, i.uv + bd * float2(-0.707, -0.707));
		    half4 a2 = tex2D(_MainTex, i.uv + bd * float2(0, -1));
		    half4 a3 = tex2D(_MainTex, i.uv + bd * float2(+0.707, -0.707));

		    half4 a4 = tex2D(_MainTex, i.uv + bd * float2(-1,  0));
		    half4 a6 = tex2D(_MainTex, i.uv + bd * float2(+1,  0));

		    half4 a7 = tex2D(_MainTex, i.uv + bd * float2(-0.707, +0.707));
		    half4 a8 = tex2D(_MainTex, i.uv + bd * float2(0, +1));
		    half4 a9 = tex2D(_MainTex, i.uv + bd * float2(+0.707, +0.707));

            kernel = 0.25 * source + 0.125 * (a2 + a4 + a6 + a8) + 0.0625 * (a1 + a3 + a7 + a9);           
        } else {
            //won't really optimize anything due to how gpus work but ehh
            kernel = source;
        }
        
        float intensity = max(kernel.r, max(kernel.g, kernel.b));

        /*
        if (intensity > 1) {
            return fixed4(1,0,0,1);
        }
        */

        intensity = intensity - 1;

        intensity = clamp(intensity, 0, 1);

        if (intensity == 0) {
            return fixed4(0,0,0,0);
        }

        kernel.a = intensity;

        return kernel;
    }

    fixed4 fragB (v2f_img i) : SV_Target
    {
        const float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
        const float2 p13_31 = float2(unity_CameraProjection._13, unity_CameraProjection._23);
        const float isOrtho = unity_OrthoParams.w;
        const float near = _ProjectionParams.y;
        const float far = _ProjectionParams.z;

        float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
        #if defined(UNITY_REVERSED_Z)
                d = 1 - d;
        #endif
        float zOrtho = lerp(near, far, d);
        float zPers = near * far / lerp(far, near, d);
        float vz = lerp(zPers, zOrtho, isOrtho);

        float3 vpos = float3((i.uv * 2 - 1 - p13_31) / p11_22 * lerp(vz, 1, isOrtho), -vz);

        _InverseView = unity_CameraToWorld;

        float4 cpos = float4(_WorldSpaceCameraPos, 0);

        //don't ask why, it just works this way
        //(real answer: camera space has inverted z axis for some reason?)
        vpos.z *= -1;

        float4 wpos = cpos + mul(_InverseView, float4(vpos, 0));

        half4 source = tex2D(_MainTex, i.uv);

        float bd = _BlurStrengthB;

        //8 samples
        half4 kernel;

        if (bd > 0) {
            //3x3 blur
            
            half4 a1 = tex2D(_MainTex, i.uv + bd * float2(-0.3826, -0.9238));
		    half4 a2 = tex2D(_MainTex, i.uv + bd * float2(0.3826, -0.9238));
		    half4 a3 = tex2D(_MainTex, i.uv + bd * float2(-0.3826, 0.9238));

		    half4 a4 = tex2D(_MainTex, i.uv + bd * float2(0.3826, 0.9238));
		    half4 a6 = tex2D(_MainTex, i.uv + bd * float2(-0.9238, -0.3826));

		    half4 a7 = tex2D(_MainTex, i.uv + bd * float2(0.9238, -0.3826));
		    half4 a8 = tex2D(_MainTex, i.uv + bd * float2(-0.9238, 0.3826));
		    half4 a9 = tex2D(_MainTex, i.uv + bd * float2(0.9238, 0.3826));

            kernel = 0.25 * source + 0.125 * (a2 + a4 + a6 + a8) + 0.0625 * (a1 + a3 + a7 + a9);
        } else {
            //won't really optimize anything due to how gpus work but ehh
            kernel = source;
        }

        float intensity = max(kernel.r, max(kernel.g, kernel.b));

        intensity = intensity - 1;

        intensity = clamp(intensity, 0, 1);

        kernel.a = intensity;
        return kernel;

    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Name "Original"
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragNeutral
            ENDCG
        }

        Pass
        {
            Name "A"
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }

        Pass
        {
            Name "B"

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragB
            ENDCG
        }
    }
}