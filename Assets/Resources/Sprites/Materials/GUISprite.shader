//shader that makes stuff appear in front
//normal shader doesn't let me do that

//Note: used for things existing in the world, not canvas sprites

Shader "Unlit/GUISprite"
{
    Properties
    {
		[PerRendererData]  _MainTex("Sprite Texture", 2D) = "white" { }
		 _Color("Color", Color) = (1.000000,1.000000,1.000000,1.000000)

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Overlay+1"}
        LOD 100

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
                return col;
            }
            ENDCG
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest Always
        }
    }
}
