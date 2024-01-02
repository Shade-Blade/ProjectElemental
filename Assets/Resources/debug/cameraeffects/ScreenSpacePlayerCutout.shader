Shader "Hidden/ScreenSpacePlayerCutout"
{
    Properties
    {
        _MainTex ("-", 2D) = ""{}
        _MinDistance("Min Distance", float) = 1
        _MaxDistance("Max Distance", float) = 1
        _Color("Color", Color) = (1.0,1.0,1.0,1.0)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D_float _CameraDepthTexture;
    float4x4 _InverseView;

    half _MinDistance;
    half _MaxDistance;
    half4 _Color;

    
	float4 _MainTex_TexelSize;

	uniform float4 _PlayerPos;

    fixed4 frag (v2f_img i) : SV_Target
    {

        fixed3 pos = normalize(_PlayerPos.xyz - _WorldSpaceCameraPos)*(_ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y))+_WorldSpaceCameraPos;
        fixed2 playerScreenPos =0;
        fixed3 toCam = mul(unity_WorldToCamera, pos);
        fixed camPosZ = toCam.z;
        fixed height = 2 * camPosZ / unity_CameraProjection._m11;
        fixed width = _ScreenParams.x / _ScreenParams.y * height;
        playerScreenPos.x = (toCam.x + width / 2)/width;
        playerScreenPos.y = (toCam.y + height / 2)/height;

        //float4 playerRelativePos = mul(unity_WorldToCamera, float4(_PlayerPos.xyz, 1.0));
        //float4 playerClipPos = mul(UNITY_MATRIX_VP, playerRelativePos);
        float4 playerClipPos = UnityObjectToClipPos(_PlayerPos.xyz);
        //float4 playerScreenPos = ComputeScreenPos(playerClipPos);
        //playerScreenPos.xy /= playerScreenPos.w;

        //playerScreenPos.x *= -10;
        playerScreenPos.xy /= _MainTex_TexelSize;
        //playerScreenPos.xy *= 0.1;
        //playerScreenPos.xy += (0.5 / _MainTex_TexelSize);


        //playerScreenPos /= -_MainTex_TexelSize;
        
        half4 source = tex2D(_MainTex, i.uv);

        float2 realCoords = i.uv / _MainTex_TexelSize;

        float sdist = length(playerScreenPos - realCoords);

        float lerpVal = (sdist - _MinDistance) / (_MaxDistance - _MinDistance);
        lerpVal = saturate(lerpVal);

        source = lerp(source, _Color, lerpVal);

        return source;
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