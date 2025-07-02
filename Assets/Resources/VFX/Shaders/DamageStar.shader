//shader that makes stuff appear in front
//normal shader doesn't let me do that

//Note: used for things existing in the world, not canvas sprites

Shader "Unlit/DamageStar"
{
    Properties
    {
		[PerRendererData]  _MainTex("Sprite Texture", 2D) = "white" { }
          _PointCount ("PointCount", float) = 5            //note: fractional numbers lead to broken stars
		 _Color("Color", Color) = (1.000000,1.000000,1.000000,1.000000)

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" 
                "IgnoreProjector"="True" 
                "Queue" = "Overlay"}
        LOD 100

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag alpha:fade

            #include "UnityCG.cginc"

            struct v2f
            {
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
				float4  color : COLOR;
            };

            sampler2D _MainTex;
			float4 _Color;
            float4 _MainTex_ST;
            float _PointCount;

            v2f vert (appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;
				//o.color *= _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb *= col.a;
				col.r *= i.color.r;
				col.g *= i.color.g;
				col.b *= i.color.b;
                col.a *= i.color.a;

                fixed radius = 0.5;
                fixed inradius = lerp(0.25, 0.35, saturate(((_PointCount - 2) / 8)));

                half timespin = (1/_PointCount) * 0.5 * round(frac(_Time.y * 3 * sqrt(_PointCount/5)));

                half angle = timespin + 0.75 + (atan2(i.uv.y - 0.5, i.uv.x - 0.5) / 6.283185307179586);
                half b = frac(angle * _PointCount);
                half c = 2 * abs(b - 0.5);
                half d = lerp(inradius,radius,pow(c,lerp(5, 2, saturate(((_PointCount - 2) / 8)))));

                if (length(i.uv - half2(0.5,0.5)) > d) {
                    col.a = 0;
                }

                return col;
            }
            ENDCG
        }
    }
}
