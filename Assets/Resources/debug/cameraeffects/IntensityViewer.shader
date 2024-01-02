//Modified from Firewatch fog effect by Harry Alisavakis https://halisavakis.com/my-take-on-shaders-firewatch-multi-colored-fog/

Shader "Custom/IntensityViewer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
 
            fixed4 frag (v2f i) : SV_Target
            {
                half4 orCol = tex2D(_MainTex, i.uv);

                half intensity = max(orCol.r, max(orCol.g, orCol.b));
                half mintensity = min(orCol.r, min(orCol.g, orCol.b));

                mintensity = clamp(-mintensity, 0, 1);

                if (intensity < 1) {
                    return fixed4(intensity, intensity, intensity + mintensity, 1);
                } else {

                    fixed red = intensity;
                    fixed green = 2 - intensity;
                    fixed blue = 2 - intensity - mintensity;

                    red = clamp(red, 0, 1);
                    green = clamp(green, 0, 1);
                    blue = clamp(blue, 0, 1);

                    return fixed4(red, green, blue, 1);
                }
            }
            ENDCG
        }
    }
}