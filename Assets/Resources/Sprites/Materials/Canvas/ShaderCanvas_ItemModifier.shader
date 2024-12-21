//Copy paste of UI/Default but with extra stuff

Shader "UI/Default_ItemModifier"
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

        _BidirCutoff("Bidirectional Cutoff", float) = 0.1		

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _OverlayColor("Overlay Color", Color) = (0,0,0,0)
		_OverlayTex("Overlay Texture", 2D) = "white" {}
		_PerPixelScale("Overlay Pixel Scale", float) = 1
		_XOffset("X Offset", Range(0,1)) = 1	//cyclic so values outside 0,1 are equivalent to something in that range
		_YOffset("Y Offset", Range(0,1)) = 1	//cyclic so values outside 0,1 are equivalent to something in that range		
		_TimeScale("Time Scale", float) = 1
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
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            sampler2D _OverlayTex;
		    fixed4 _OverlayColor;
		    float _PerPixelScale;

		    float _TimeScale;
		    float _XOffset;
		    float _YOffset;


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

                fixed4 b = fixed4(0,0,0,0);
		        if (c.a != 0) {
			        float timeFactor = _Time.x * _TimeScale;

			        float2 buv = IN.texcoord;
			        buv.x /= _PerPixelScale * 2;
			        buv.y /= _PerPixelScale;

			        buv.x = buv.x + _XOffset;
			        buv.y = buv.y + _YOffset;
			        //buv.y = buv.y - timeFactor;

			        //need to add a correction factor because red is too dim compared to cyan
			        float bc = max(1, 3 - _OverlayColor.x - _OverlayColor.y - _OverlayColor.z);

			        float br = tex2D(_OverlayTex, buv);

			        //Time factor is used to cycle things
			        br += timeFactor;
			        br = frac(br);

			        b = (br * _OverlayColor * bc) * 0.8;
			        b *= 0.8 + 0.2 * sin(timeFactor * 1.2);
		        }
		        //o.Emission = b;
                c += b;

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