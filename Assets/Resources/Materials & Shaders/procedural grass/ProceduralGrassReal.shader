Shader "Custom/ProceduralGrassReal"
{
	Properties
	{
		[HDR]
		_BaseColor("Base Color", Color) = (0, 0, 0, 1)
		[HDR]
		_TipColor("Tip Color", Color) = (1, 1, 1, 1)
		_TranslucentGain("Translucent Gain", float) = 0.25
		_BaseTex("Base Texture", 2D) = "white" {}
	}
    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
			"Queue" = "Geometry"
        }

        CGINCLUDE
			//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			//#pragma multi_compile _ _SHADOWS_SOFT
            #include "UnityCG.cginc"
            #include "Autolight.cginc"

			struct appdata
			{
				uint vertexID : SV_VertexID;
				uint instanceID : SV_InstanceID;
			};

			struct v2f
			{
				float4 positionCS : SV_Position;
				float4 positionWS : TEXCOORD0;
				float2 uv : TEXCOORD1;
				float3 normal : NORMAL;
                float3 vertexLighting : TEXCOORD3;
			};

			StructuredBuffer<float3> _Positions;
			StructuredBuffer<float3> _Normals;
			StructuredBuffer<float2> _UVs;
			StructuredBuffer<float4x4> _TransformMatrices;

			CBUFFER_START(UnityPerMaterial)
				float4 _BaseColor;
				float4 _TipColor;
				sampler2D _BaseTex;
				float4 _BaseTex_ST;

				float _Cutoff;
				float _TranslucentGain;
			CBUFFER_END
		ENDCG

        Pass
		{
			Name "GrassPass"
			Tags { "LightMode" = "ForwardBase" }

	        CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			v2f vert(appdata v)
			{
				v2f o;

				float4 positionOS = float4(_Positions[v.vertexID], 1.0f);
				float4x4 objectToWorld = _TransformMatrices[v.instanceID];

				o.positionWS = mul(objectToWorld, positionOS);
				o.positionCS = mul(UNITY_MATRIX_VP, o.positionWS);
				o.uv = _UVs[v.vertexID];
				o.normal = UnityObjectToWorldNormal(_Normals[v.vertexID]); //normalize(mul(objectToWorld, _Normals[v.vertexID]).xyz);

				//o.normal = normalize(float3(v.vertexID,0,1));

				float3 world = o.positionWS;

                o.vertexLighting = float3(0.0, 0.0, 0.0);

                for (int index = 0; index < 4; index++)
                {  
                    float4 lightPosition = float4(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index], 1.0);
                    float3 vertexToLightSource = lightPosition.xyz - world.xyz;

                    float3 lightDirection = normalize(vertexToLightSource);
                    float squaredDistance = dot(vertexToLightSource, vertexToLightSource);

                    float attenuation = 1.0 / (1.0 + unity_4LightAtten0[index] * squaredDistance);
                
                    float invert = 1;
                    if (unity_LightColor[index].a == 0) {
                        invert = -1;
                    }

                    float3 diffuseReflection = attenuation * invert
                    * unity_LightColor[index].rgb * max(0.0, dot(lerp(o.normal, -normalize(world.xyz - lightPosition.xyz), _TranslucentGain), lightDirection));
                
                    o.vertexLighting = o.vertexLighting + diffuseReflection;
                }

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 color = tex2D(_BaseTex, i.uv);

				//not working :(

				//float3 normal = normalize(i.normal);
				//float light = saturate(dot(normalize(_WorldSpaceLightPos0), normal)) * 0.5 + 0.5;

				//float factor = sin(_Time.w);

				//normal += (factor, factor, factor);

				//return float4(normal.r,normal.g,normal.b,1); //float4(normal.r, normal.g, normal.b, 1);//float4(normal, 1);//color * lerp(_BaseColor, _TipColor, i.uv.y) * (light);
				//return float4(i.normal.x,i.normal.y,i.normal.z,1);
				return color * lerp(_BaseColor, _TipColor, i.uv.y) + float4(i.vertexLighting, 0);
			}
			
	        ENDCG
        }

        Pass
		{
			Name "GrassShadow"
			Tags {
                "LightMode" = "ShadowCaster"
            }

	        CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragShadow

			v2f vert(appdata v)
			{
				v2f o;

				float4 positionOS = float4(_Positions[v.vertexID], 1.0f);
				float4x4 objectToWorld = _TransformMatrices[v.instanceID];

				o.positionWS = mul(objectToWorld, positionOS);
				o.positionCS = mul(UNITY_MATRIX_VP, o.positionWS);
				o.uv = _UVs[v.vertexID];

				return o;
			}

			float4 fragShadow(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }  
			
	        ENDCG
        }
    }
    FallBack "Diffuse"
}
