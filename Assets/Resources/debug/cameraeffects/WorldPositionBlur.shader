Shader "Hidden/WorldPositionBlur"
{
    Properties
    {
        _MainTex ("-", 2D) = ""{}
        _MinDistance("Min Distance Blurred", float) = 1
        _MaxDistance("Max Distance Blurred", float) = 1
        _BlurStrength("Blur Strength", float) = 0.04
        _BlurStrengthB("Blur Strength", float) = 0.06
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D_float _CameraDepthTexture;
    float4x4 _InverseView;

    half _MinDistance;
    half _MaxDistance;
    half _BlurStrength;
    half _BlurStrengthB;

	uniform float4 _PlayerPos;

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

        float distance = length(wpos - _PlayerPos);

        float realDist = (distance - _MinDistance) / (_MaxDistance - _MinDistance);
        realDist = saturate(realDist) * _BlurStrength;

        float bd = realDist;

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
            
            //kernel = 8 * source + -1 * (a2 + a4 + a6 + a8 + a1 + a3 + a7 + a9);

            //5x5 blur
            //A bit sussy to sample stuff 25 times :/
            /*
            half4 a11 = tex2D(_MainTex, i.uv + bd * float2(-2, 2));
            half4 a12 = tex2D(_MainTex, i.uv + bd * float2(-1, 2));
            half4 a13 = tex2D(_MainTex, i.uv + bd * float2(0, 2));
            half4 a14 = tex2D(_MainTex, i.uv + bd * float2(1, 2));
            half4 a15 = tex2D(_MainTex, i.uv + bd * float2(2, 2));
            half4 a21 = tex2D(_MainTex, i.uv + bd * float2(-2, 1));
            half4 a22 = tex2D(_MainTex, i.uv + bd * float2(-1, 1));
            half4 a23 = tex2D(_MainTex, i.uv + bd * float2(0, 1));
            half4 a24 = tex2D(_MainTex, i.uv + bd * float2(1, 1));
            half4 a25 = tex2D(_MainTex, i.uv + bd * float2(2, 1));
            half4 a31 = tex2D(_MainTex, i.uv + bd * float2(-2, 0));
            half4 a32 = tex2D(_MainTex, i.uv + bd * float2(-1, 0));
            half4 a33 = tex2D(_MainTex, i.uv + bd * float2(0, 0));
            half4 a34 = tex2D(_MainTex, i.uv + bd * float2(1, 0));
            half4 a35 = tex2D(_MainTex, i.uv + bd * float2(2, 0));
            half4 a41 = tex2D(_MainTex, i.uv + bd * float2(-2, -1));
            half4 a42 = tex2D(_MainTex, i.uv + bd * float2(-1, -1));
            half4 a43 = tex2D(_MainTex, i.uv + bd * float2(0, -1));
            half4 a44 = tex2D(_MainTex, i.uv + bd * float2(1, -1));
            half4 a45 = tex2D(_MainTex, i.uv + bd * float2(2, -1));
            half4 a51 = tex2D(_MainTex, i.uv + bd * float2(-2, -2));
            half4 a52 = tex2D(_MainTex, i.uv + bd * float2(-1, -2));
            half4 a53 = tex2D(_MainTex, i.uv + bd * float2(0, -2));
            half4 a54 = tex2D(_MainTex, i.uv + bd * float2(1, -2));
            half4 a55 = tex2D(_MainTex, i.uv + bd * float2(2, -2));

            kernel = 0.00390625 * (a11 + a15 + a51 + a55) + 0.015625 * (a12 + a21 + a14 + a41 + a25 + a52 + a45 + a54) + 0.0234375 * (a13 + a31 + a53 + a35) + 0.0625 * (a22 + a24 + a42 + a44) + 0.09375 * (a23 + a32 + a43 + a34) + 0.140625 * a33;
            */


            //Weird 13 blur
            /*
            float offset1 = 1.41176470; 
		    float offset2 = 3.29411764;
		    float offset3 = 5.17647058;

            half4 ay1u = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            half4 ay2u = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            half4 ay3u = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            half4 ay1d = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            half4 ay2d = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            half4 ay3d = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            */
        } else {
            //won't really optimize anything due to how gpus work but ehh
            kernel = source;
        }
        
        kernel.a = 1;
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

        float distance = length(wpos - _PlayerPos);

        float realDist = (distance - _MinDistance) / (_MaxDistance - _MinDistance);
        realDist = saturate(realDist) * _BlurStrengthB;

        float bd = realDist;

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
            
            //kernel = 8 * source + -1 * (a2 + a4 + a6 + a8 + a1 + a3 + a7 + a9);

            //5x5 blur
            //A bit sussy to sample stuff 25 times :/
            /*
            half4 a11 = tex2D(_MainTex, i.uv + bd * float2(-2, 2));
            half4 a12 = tex2D(_MainTex, i.uv + bd * float2(-1, 2));
            half4 a13 = tex2D(_MainTex, i.uv + bd * float2(0, 2));
            half4 a14 = tex2D(_MainTex, i.uv + bd * float2(1, 2));
            half4 a15 = tex2D(_MainTex, i.uv + bd * float2(2, 2));
            half4 a21 = tex2D(_MainTex, i.uv + bd * float2(-2, 1));
            half4 a22 = tex2D(_MainTex, i.uv + bd * float2(-1, 1));
            half4 a23 = tex2D(_MainTex, i.uv + bd * float2(0, 1));
            half4 a24 = tex2D(_MainTex, i.uv + bd * float2(1, 1));
            half4 a25 = tex2D(_MainTex, i.uv + bd * float2(2, 1));
            half4 a31 = tex2D(_MainTex, i.uv + bd * float2(-2, 0));
            half4 a32 = tex2D(_MainTex, i.uv + bd * float2(-1, 0));
            half4 a33 = tex2D(_MainTex, i.uv + bd * float2(0, 0));
            half4 a34 = tex2D(_MainTex, i.uv + bd * float2(1, 0));
            half4 a35 = tex2D(_MainTex, i.uv + bd * float2(2, 0));
            half4 a41 = tex2D(_MainTex, i.uv + bd * float2(-2, -1));
            half4 a42 = tex2D(_MainTex, i.uv + bd * float2(-1, -1));
            half4 a43 = tex2D(_MainTex, i.uv + bd * float2(0, -1));
            half4 a44 = tex2D(_MainTex, i.uv + bd * float2(1, -1));
            half4 a45 = tex2D(_MainTex, i.uv + bd * float2(2, -1));
            half4 a51 = tex2D(_MainTex, i.uv + bd * float2(-2, -2));
            half4 a52 = tex2D(_MainTex, i.uv + bd * float2(-1, -2));
            half4 a53 = tex2D(_MainTex, i.uv + bd * float2(0, -2));
            half4 a54 = tex2D(_MainTex, i.uv + bd * float2(1, -2));
            half4 a55 = tex2D(_MainTex, i.uv + bd * float2(2, -2));

            kernel = 0.00390625 * (a11 + a15 + a51 + a55) + 0.015625 * (a12 + a21 + a14 + a41 + a25 + a52 + a45 + a54) + 0.0234375 * (a13 + a31 + a53 + a35) + 0.0625 * (a22 + a24 + a42 + a44) + 0.09375 * (a23 + a32 + a43 + a34) + 0.140625 * a33;
            */


            //Weird 13 blur
            /*
            float offset1 = 1.41176470; 
		    float offset2 = 3.29411764;
		    float offset3 = 5.17647058;

            half4 ay1u = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            half4 ay2u = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            half4 ay3u = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            half4 ay1d = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            half4 ay2d = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            half4 ay3d = tex2D(_MainTex, i.uv + bd * float2(0, offset1));
            */
        } else {
            //won't really optimize anything due to how gpus work but ehh
            kernel = source;
        }

        kernel.a = 0.5;
        return kernel;
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
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