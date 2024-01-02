Shader "Hidden/WorldPositionDualFog"
{
    Properties
    {
        _MainTex ("-", 2D) = ""{}

        _FogIntensity("Fog intensity", float) = 1
        _FogStart("Fog start distance", float) = 1
        _FogEnd("Fog end distance", float) = 1
        _FogMid1("Fog mid proportion 1", float) = 1
        _FogMid2("Fog mid proportion 2", float) = 1
        _FogColor1("Fog Color 1", Color) = (1.0,1.0,1.0,1.0)
        _FogColor2("Fog Color 2", Color) = (1.0,1.0,1.0,1.0)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D_float _CameraDepthTexture;
    float4x4 _InverseView;
    
    float _FogIntensity;
    float _FogStart;
    float _FogEnd;
    float _FogMid1;
    float _FogMid2;

    fixed4 _FogColor1;
    fixed4 _FogColor2;

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

        half4 orCol = tex2D(_MainTex, i.uv);
        //half3 color = pow(abs(cos(wpos.xyz * UNITY_PI * 4)), 20);
        //return half4(lerp(source.rgb, color, _Intensity), source.a);

        float distance = length(wpos - _PlayerPos);

        float fogValue = (distance - _FogStart)/_FogEnd;

        float interp = (fogValue - _FogMid1) / (_FogMid2 - _FogMid1);
        interp = clamp(interp, 0, 1);

        fixed4 fogCol = lerp(_FogColor1, _FogColor2, interp); //tex2D(_ColorRamp, (float2(fogValue, 0)));

        fogValue = clamp(fogValue, 0, 1);
        fogCol.a = lerp(0, 1, fogValue);

        return lerp(orCol, fogCol, fogCol.a * _FogIntensity);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}