//Copy paste of UI/Default but with extra stuff

Shader "UI/Default_SpriteHSVAlter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _TopColor ("Top Color", Color) = (1,1,1,1)
        _TopUse("Top Use", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
                
        _Saturation("Saturation", float) = 0.1		
        _Value("Value", float) = 0.1			
        _Threshold("Threshold", float) = 0.9

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
            fixed4 _Color;
            fixed4 _TopColor;
            fixed _TopUse;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _Saturation;
            float _Value;
            float _Threshold;


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
                half4 c = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
                
                half oldma = max(c.r, max(c.r, c.b));

                c *= IN.color;
                half ma = max(c.r, max(c.g, c.b));
                half mi = min(c.r, min(c.g, c.b));
                half delta = ma - mi;

                half4 oldc = c;

                if (ma == mi) {
                    //c = oldc;
                    _Saturation = 0;
                }
                _Saturation += delta;
                _Value += ma;

                half olda = c.a;
                c -= half4(mi, mi, mi, 0);
                if (delta > 0) {
                    c /= half4(delta, delta, delta, 1);
                }
                c *= half4(_Saturation, _Saturation, _Saturation, 1);
                half inverse = 1 - _Saturation;
                c += half4(inverse, inverse, inverse, 1);
                c *= half4(_Value, _Value, _Value, 1);
                c.a = olda;

                c = lerp(c, lerp(oldc, _TopColor, _TopUse), saturate((oldma - _Threshold)/(1 - _Threshold)));

                //c = half4(saturate((ma - _Threshold)/(1 - _Threshold)), 1, 1, 1);

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