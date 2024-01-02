// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DistortingGrabPass" {
    Properties {
        _Intensity ("Intensity", float) = 0
        _Omega ("Omega", float) = 0
    }
    SubShader
    {
        // Draw after all opaque geometry
        Tags { "Queue" = "Transparent" }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }


        // Render the object with the texture generated above, and invert the colors
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                //float4 grabPosDrift : TEXCOORD0;
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_base v) {
                v2f o;
                // use UnityObjectToClipPos from UnityCG.cginc to calculate 
                // the clip-space of the vertex
                o.pos = UnityObjectToClipPos(v.vertex);

                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }

            sampler2D _BackgroundTexture;
            sampler2D _CameraDepthTexture;

            float _Intensity;
            float _Omega;

            half4 frag(v2f i) : SV_Target
            {
                //float4 newgrabpos = i.grabPos;
                //newgrabpos.x += sin((_Time.y + newgrabpos.y) * _Intensity)/20;

                float4 checkPos = i.grabPos;
                checkPos.x += _Intensity * sin((_Time.y + checkPos.y) * _Omega)/20;

                fixed refrFix = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(checkPos)));

                half4 bgcolor;

                if (LinearEyeDepth(refrFix) < i.grabPos.w) {
                    bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos);
                }
                else {
                    bgcolor = tex2Dproj(_BackgroundTexture, checkPos);
                }

                return bgcolor;
            }
            ENDCG
        }

    }
    FallBack "Diffuse"
}