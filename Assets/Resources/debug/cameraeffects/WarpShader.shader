//warps the screen around (outer color is used for outside) (Note: removing that code will cause some weird edge effects)
//May be a better idea to apply a vignette or some other screenspace smoothing near the edges to hide the edge

Shader "Custom/WarpShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale("Scale", float) = 1
        _UVScale("UV Scale", float) = 1
        _OuterColor("Outer Color", Color) = (1.0,1.0,1.0,1.0)
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
                //float4 scrPos : TEXCOORD1;
            };
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                //o.scrPos = ComputeScreenPos(o.vertex);
                return o;
            }
             
            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            float _Scale;
            float _UVScale;
            fixed4 _OuterColor;
            
 
            fixed4 frag (v2f i) : SV_Target
            {
                //fixed4 orCol = tex2D(_MainTex, i.uv);
                //float rawDepth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos));
                //float depthValue = LinearEyeDepth (rawDepth);

                float timeFactor = _Time.z;

                float xsin = sin(timeFactor + _UVScale * i.uv.x);
                float ysin = cos(timeFactor + _UVScale * i.uv.y);

                i.uv.x = i.uv.x + (xsin * _Scale);
                i.uv.y = i.uv.y + (ysin * _Scale);

                fixed4 orCol = tex2D(_MainTex, i.uv);

                if (i.uv.x < 0 || i.uv.y < 0 || i.uv.x > 1 || i.uv.y > 1) {
                    return _OuterColor;
                } else {
                    return orCol;
                }
            }
            ENDCG
        }
    }
}