// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BlackHole" {
    Properties {
        _FresnelPower ("Fresnel Power", float) = 0
        _HoleValue ("Hole Value", float) = 1
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
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                //float4 grabPosDrift : TEXCOORD0;
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
				float3 viewDir : TEXCOORD1;
                float4 worldPos : POSITION2;
            };

            v2f vert(appdata_base v) {
                v2f o;
                // use UnityObjectToClipPos from UnityCG.cginc to calculate 
                // the clip-space of the vertex
                o.pos = UnityObjectToClipPos(v.vertex);

                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
                o.grabPos = ComputeGrabScreenPos(o.pos);

                o.normal = v.normal;
                o.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            sampler2D _BackgroundTexture;
            sampler2D _CameraDepthTexture;

            float _FresnelPower;
            float _HoleValue;

            half4 frag(v2f i) : SV_Target
            {
                //float4 newgrabpos = i.grabPos;
                //newgrabpos.x += sin((_Time.y + newgrabpos.y) * _Intensity)/20;

                float4 checkPos = i.grabPos;                
                //checkPos.x += _Intensity * sin((_Time.y + checkPos.y) * _Omega)/20;

                float fresnel = dot(-i.normal, i.viewDir);
                fresnel = saturate(1 - fresnel);
                fresnel = pow((1 - fresnel), _FresnelPower);

                if (fresnel * _HoleValue > 1) {
                    return half4(0,0,0,1);
                }

                //note: cull front means that the normals already face inward
                float3 diff = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, -i.normal));

                diff *= fresnel;

                checkPos.x += diff.x;
                checkPos.y += diff.y;

                //return half4(i.normal.x, i.normal.y, i.normal.z, 1);
                //return half4(i.viewDir.x, i.viewDir.y, i.viewDir.z, 1);
                //return half4(fresnel, fresnel, fresnel, 1);
                //return half4(0.5 + diff.x, 0.5 + diff.y, 0.5 + diff.z, 1);



                fixed refrFix = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(checkPos)));

                half4 bgcolor;

                if (LinearEyeDepth(refrFix) < i.grabPos.w) {
                    bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos);
                }
                else {
                    bgcolor = tex2Dproj(_BackgroundTexture, checkPos);
                }

                return half4(bgcolor.xyz, 1);
            }
            ENDCG
        }

    }
    FallBack "Diffuse"
}