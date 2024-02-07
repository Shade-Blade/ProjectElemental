Shader "Custom/ZWriterTest"
{
    SubShader
    {
        //Result: instead of skybox you get wacky stuff
        //probably just random gpu data
        //It creates a lot of bloom though

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Background"
        }

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;      
            };

            struct v2f
            {
				//float3 viewDir : TEXCOORD1;
            };

            v2f vert(appdata v, out float4 vertex : SV_POSITION)
            {
                v2f o;
                vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            half4 frag (v2f i, UNITY_VPOS_TYPE vpos : VPOS) : SV_Target
            {
                return half4(0,0,0,0);
            }
            ENDCG
        }
    }
}