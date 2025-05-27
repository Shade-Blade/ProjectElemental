//Copy paste of UI/Default but with extra stuff

Shader "UI/Default_SpriteForceHue"
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
                
        _Hue("Hue", float) = 0.1			
        _SaturationMult("SaturationMult", float) = 0.1		

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
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _Hue;
            float _SaturationMult;

            float3 hue2rgb(float hue) {
			    hue = frac(hue); //only use fractional part of hue, making it loop
			    float r = abs(hue * 6 - 3) - 1; //red
			    float g = 2 - abs(hue * 6 - 2); //green
			    float b = 2 - abs(hue * 6 - 4); //blue
			    float3 rgb = float3(r, g, b); //combine components
			    rgb = saturate(rgb); //clamp between 0 and 1
			    return rgb;
		    }

		    float3 hsv2rgb(float3 hsv)
		    {
			    float3 rgb = hue2rgb(hsv.x); //apply hue
			    rgb = lerp(1, rgb, hsv.y); //apply saturation
			    rgb = rgb * hsv.z; //apply value
			    return rgb;
		    }

		    float3 rgb2hsv(float3 rgb)
		    {
			    float maxComponent = max(rgb.r, max(rgb.g, rgb.b));
			    float minComponent = min(rgb.r, min(rgb.g, rgb.b));
			    float diff = maxComponent - minComponent;
			    float hue = 0;
			    if (maxComponent == rgb.r) {
				    hue = 0 + (rgb.g - rgb.b) / diff;
			    }
			    else if (maxComponent == rgb.g) {
				    hue = 2 + (rgb.b - rgb.r) / diff;
			    }
			    else if (maxComponent == rgb.b) {
				    hue = 4 + (rgb.r - rgb.g) / diff;
			    }
			    if (diff == 0) {
				    hue = 0; //fix div by 0
			    }
			    hue = frac(hue / 6);
			    float saturation = diff / maxComponent;
			    float value = maxComponent;
			    return float3(hue, saturation, value);
		    }


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
                
                float3 hsv = rgb2hsv(c.rgb);

                hsv.x = _Hue;
                hsv.y *= _SaturationMult;

                c = half4(hsv2rgb(hsv), c.a);

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