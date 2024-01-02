Shader "Custom/FogPlane"
{
    Properties
    {
        _Tint("Fog Tint", Color) = (1, 1, 1, .5)
        _SecondTint("Secondary Tint", Color) = (1, 1, 1, .5)
        _Strength("Fog Strength", float) = 0.5
        _GradientStart("Fog Gradient Start", float) = 0.5 
        _GradientEnd("Fog Gradient End", float) = 0.5 
    }
        SubShader
    {
        Tags { 
            "RenderType" = "Opaque"  
            "Queue" = "Transparent" 
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
 
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
            };
 
            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 scrPos : TEXCOORD2;//
            };
 
            float4 _Tint;
            float4 _SecondTint;
            uniform sampler2D _CameraDepthTexture; //Depth Texture
            float _Strength;
            float _GradientStart;
            float _GradientEnd;
 
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.scrPos = ComputeScreenPos(o.vertex); // grab position on screen
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
 
            fixed4 frag(v2f i) : SV_Target
            {
                half depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos))); // depth
                half4 fog = (_Strength * (depth - i.scrPos.w));// fog by comparing depth and screenposition
                
                float gradVal = fog.w - _GradientStart;
                gradVal = gradVal / (_GradientEnd - _GradientStart);
                gradVal = saturate(gradVal);
                float4 gradTint = lerp(_Tint, _SecondTint, gradVal);

                fog = saturate(fog);

                half4 col = fog * gradTint;// add the color
                col = saturate(col);// clamp to prevent weird artifacts
                UNITY_APPLY_FOG(i.fogCoord, col); // comment out this line if you want this fog to override the fog in lighting settings
                return col;
            }
            ENDCG
        }
    }
}