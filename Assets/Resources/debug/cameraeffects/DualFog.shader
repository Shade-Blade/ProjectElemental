//Modified from Firewatch fog effect by Harry Alisavakis https://halisavakis.com/my-take-on-shaders-firewatch-multi-colored-fog/

Shader "Custom/DualFog"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogIntensity("Fog intensity", float) = 1
        _FogStart("Fog start distance", float) = 1
        _FogEnd("Fog end distance", float) = 1
        _FogMid1("Fog mid proportion 1", float) = 1
        _FogMid2("Fog mid proportion 2", float) = 1
        _FogColor1("Fog Color 1", Color) = (1.0,1.0,1.0,1.0)
        _FogColor2("Fog Color 2", Color) = (1.0,1.0,1.0,1.0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
             
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
             
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 scrPos : TEXCOORD1;
            };
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.scrPos = ComputeScreenPos(o.vertex);
                return o;
            }
             
            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float _FogIntensity;
            float _FogStart;
            float _FogEnd;
            float _FogMid1;
            float _FogMid2;

            fixed4 _FogColor1;
            fixed4 _FogColor2;
 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 orCol = tex2D(_MainTex, i.uv);
                float rawDepth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos));
                float depthValue = LinearEyeDepth (rawDepth);
                float fogValue = (depthValue-_FogStart)/_FogEnd;

                float interp = (fogValue - _FogMid1) / (_FogMid2 - _FogMid1);
                interp = clamp(interp, 0, 1);

                fixed4 fogCol = lerp(_FogColor1, _FogColor2, interp); //tex2D(_ColorRamp, (float2(fogValue, 0)));

                fogValue = clamp(fogValue, 0, 1);
                fogCol.a = lerp(0, 1, fogValue);

                return (rawDepth > 0) ? lerp(orCol, fogCol, fogCol.a * _FogIntensity) : _FogColor2;
            }
            ENDCG
        }
    }
}