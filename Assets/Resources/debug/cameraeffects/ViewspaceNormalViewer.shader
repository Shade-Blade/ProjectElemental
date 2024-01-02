Shader "Hidden/ViewspaceNormalViewer"
{
    Properties
    {
        _MainTex ("-", 2D) = ""{}
		_Intensity("Intensity", float) = 0.5
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D_float _CameraDepthNormalsTexture;
    float4x4 _InverseView;
    half _Intensity;

    float4 frag (v2f_img i) : SV_Target
    {
        float depth;
        float3 normals;
        DecodeDepthNormal( tex2D(_CameraDepthNormalsTexture, i.uv), depth, normals);
        //float d = SAMPLE_DEPTH_TEXTURE( _CameraDepthNormalsTexture, i.uv);
        //#if defined(UNITY_REVERSED_Z)
        //        d = 1 - d;
        //#endif

        return float4(0.5 * normals + float3(0.5, 0.5, 0.5),1);

        //half4 source = tex2D(_MainTex, i.uv);
        //half3 color = pow(abs(wpos.xyz), 20); 
        //half3 color = pow(abs(cos(wpos.xyz * UNITY_PI * 4)), 20);
        //return half4(lerp(source.rgb, color, _Intensity), source.a);
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