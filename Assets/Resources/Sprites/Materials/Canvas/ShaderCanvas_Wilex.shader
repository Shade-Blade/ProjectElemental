//Copy paste of UI/Default but with extra stuff

Shader "UI/Default_Wilex"
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
		_RibbonCutoffA("Ribbon Cutoff A", float) = 0.75
		_RibbonCutoffB("Ribbon Cutoff B", float) = 0.85
		_WeaponCutoffA("Weapon Cutoff A", float) = 0.55
		_WeaponCutoffB("Weapon Cutoff B", float) = 0.7
		_WeaponCutoffC("Weapon Cutoff C", float) = 0.85

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
		    float4 _MainTex_TexelSize;

            fixed _RibbonCutoffA;
		    fixed _RibbonCutoffB;
		    fixed _WeaponCutoffA;
		    fixed _WeaponCutoffB;
		    fixed _WeaponCutoffC;

		    half4 _WRibbonColorA;
		    half4 _WRibbonColorB;
		    half4 _WRibbonColorC;

		    fixed _WRibbonMimic;
		    fixed _WRibbonRainbow;

		    half4 _WWeaponColorA;
		    half4 _WWeaponColorB;
		    half4 _WWeaponColorC;
		    half4 _WWeaponColorD;

            //color ramp thing
            fixed3 color_ramp(float w) {
                w -= floor(w);
                w *= 6;

                fixed3 output = fixed3(0,0,0);
                output[0] = abs(w - 3) - 1;
                output[1] = -abs(w - 2) + 2;
                output[2] = -abs(w - 4) + 2;
                output = saturate(output);

                return output;
            }

            half4 colorchange(half4 c, half x) {
			    //calculate weapon mask stuff early so that blue ribbons don't trigger this
			    half bl = c.b;
			    half bdelta = c.b - max(c.r, c.g);

			    //Ribbon mask
			    half rbm = max(c.r, c.b);
			    half rdelta = abs(c.r - c.b);

			    if (rbm > 0.08 && rdelta < 0.08 && c.g  < rbm / 2) {
				    if (rbm < _RibbonCutoffA) {
					    c = _WRibbonColorA;
				    } else if (rbm < _RibbonCutoffB) {
					    c = _WRibbonColorB;
				    } else {
					    c = _WRibbonColorC;
				    }
			
				    if (_WRibbonMimic == 1) {
					    fixed timeDelta = 0.15 * (1 + sin(_Time.y));
					    c = lerp(c, fixed4(0.71, 0.369, 0.965, 1), timeDelta);
				    }
				    if (_WRibbonRainbow == 1) {
					    //reinterpret c
					    //new color = (r channel * rainbow + g channel * gray)
					    c = half4(c.r * color_ramp(_Time.x * 2 + 4 * x) + c.g * half3(1,1,1), 1);
					    //c *= fixed4(color_ramp(_Time.x * 2 + 4 * IN.uv_MainTex.x), 1);
				    }
			    }
			
			    //weapon mask
			    if (bl > 0.08 && bdelta > 0.08 && max(c.r, c.g) < bl / 2) {
				    if (bl < _WeaponCutoffA) {
					    c = _WWeaponColorA;
				    } else if (bl < _WeaponCutoffB) {
					    c = _WWeaponColorB;
				    } else if (bl < _WeaponCutoffC) {
					    c = _WWeaponColorC;
				    } else {
					    c = _WWeaponColorD;
				    }
			    }

			    return c;
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
                float2 halfTexel = _MainTex_TexelSize.xy * 0.5;

			    half4 c_ul = tex2D(_MainTex, IN.texcoord + float2(-halfTexel.x, halfTexel.y));
			    c_ul = colorchange(c_ul, IN.texcoord.x);
			    half4 c_ur = tex2D(_MainTex, IN.texcoord + float2(halfTexel.x, halfTexel.y));
			    c_ur = colorchange(c_ur, IN.texcoord.x);
			    half4 c_dl = tex2D(_MainTex, IN.texcoord + float2(-halfTexel.x, -halfTexel.y));
			    c_dl = colorchange(c_dl, IN.texcoord.x);
			    half4 c_dr = tex2D(_MainTex, IN.texcoord + float2(halfTexel.x, -halfTexel.y));
			    c_dr = colorchange(c_dr, IN.texcoord.x);

                float2 uv_blend = frac(IN.texcoord * _MainTex_TexelSize.zw + 0.5);
			    half4 c = lerp(
				    lerp(c_dl, c_dr, uv_blend.x),
				    lerp(c_ul, c_ur, uv_blend.x),
				    uv_blend.y);

                c *= IN.color;

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