//Copy paste of UI/Default but with extra stuff

Shader "UI/Default_SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
	
		_Width("Width", Float) = 1
        _OutlineColor ("OutlineColor", Color) = (1,1,1,1)

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
		    float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            fixed4 _OutlineColor;

            
    		float _Width;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 c = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                // Simple sobel filter for the alpha channel.
			    float d = _MainTex_TexelSize.xy * _Width;

			    half a1 = tex2D(_MainTex, IN.texcoord + d * float2(-1, -1)).a;
			    half a2 = tex2D(_MainTex, IN.texcoord + d * float2(0, -1)).a;
			    half a3 = tex2D(_MainTex, IN.texcoord + d * float2(+1, -1)).a;

			    half a4 = tex2D(_MainTex, IN.texcoord + d * float2(-1,  0)).a;
			    half a6 = tex2D(_MainTex, IN.texcoord + d * float2(+1,  0)).a;

			    half a7 = tex2D(_MainTex, IN.texcoord + d * float2(-1, +1)).a;
			    half a8 = tex2D(_MainTex, IN.texcoord + d * float2(0, +1)).a;
			    half a9 = tex2D(_MainTex, IN.texcoord + d * float2(+1, +1)).a;

			    float gx = -a1 - a2 * 2 - a3 + a7 + a8 * 2 + a9;
			    float gy = -a1 - a4 * 2 - a7 + a3 + a6 * 2 + a9;
			
			    float w = sqrt(gx * gx + gy * gy) / 4;


                if (w > 0 || c.a > 0) {
				    // Mix the contour color.
				    half4 end = half4(lerp(c.rgb, _OutlineColor.rgb, w), 1);
                    c = end;
			    }
			    else {
                    c.a = 0;
                    c.rgb = _OutlineColor;
			    }


                #ifdef UNITY_UI_CLIP_RECT
                c.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (c.a - 0.001);
                #endif

                return c;
            }
        ENDCG
        }
    }
}