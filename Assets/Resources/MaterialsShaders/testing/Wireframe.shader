///an update to the builtin VR Spatial Mapping shader
//Shader "Xibanya/Unlit/SpatialMapping/XibWireframe"
Shader "Custom/Wireframe"
{
	Properties
	{
		[Toggle(SPATIAL_COLOR)]
		_SpatialColor	("Spatial Color?", float) = 0
		[HDR] _Color	("Color (if not using spatial color)", Color) = (1, 1, 1, 1)
		_WireThickness	("Wire Thickness", RANGE(0, 800)) = 100
		[Toggle(WIRE_ONLY)] 
		_WireOnly		("Wire only?", float) = 0
		[Toggle(_ALPHATEST_ON)] 
		_Cutout			("Cutout?", float) = 0
		_Cutoff			("Cutoff", Range(0, 1)) = 0.5
		_MainTex		("Cutout tex", 2D) = "white" {}
		[Enum(Off,0,Front,1,Back,2)]
		_Cull			("Cull", int) = 2
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Cull[_Cull]
		LOD 100
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma shader_feature_local SPATIAL_COLOR
			#pragma shader_feature_local WIRE_ONLY
			#pragma shader_feature_local _ALPHATEST_ON

			#include "UnityCG.cginc"

			float		_WireThickness;
			half4		_Color;
#ifdef _ALPHATEST_ON
			half		_Cutoff;
			sampler2D	_MainTex;
			float4		_MainTex_ST;
#endif

			struct appdata
			{
				float4 vertex	: POSITION;
#ifdef _ALPHATEST_ON
				float2 uv		: TEXCOORD0;
#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2g
			{
				float4 pos		: SV_POSITION;
				float4 worldPos : TEXCOORD0;
#ifdef _ALPHATEST_ON
				float2 uv		: TEXCOORD1;
#endif
				UNITY_VERTEX_OUTPUT_STEREO_EYE_INDEX
			};

			struct g2f
			{
				float4 pos		: SV_POSITION;
				float4 worldPos : TEXCOORD0;
				float4 dist		: TEXCOORD1;
#ifdef _ALPHATEST_ON
				float2 uv		: TEXCOORD2;
#endif
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2g vert(appdata v)
			{
				v2g o;
				UNITY_INITIALIZE_OUTPUT(v2g, o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT_STEREO_EYE_INDEX(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
#ifdef _ALPHATEST_ON
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
#endif
				return o;
			}

			[maxvertexcount(3)]
			void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i[0]);

				float2 p0 = i[0].pos.xy / i[0].pos.w;
				float2 p1 = i[1].pos.xy / i[1].pos.w;
				float2 p2 = i[2].pos.xy / i[2].pos.w;

				float2 edge0 = p2 - p1;
				float2 edge1 = p2 - p0;
				float2 edge2 = p1 - p0;

				// To find the distance to the opposite edge, we take the
				// formula for finding the area of a triangle Area = Base/2 * Height,
				// and solve for the Height = (Area * 2)/Base.
				// We can get the area of a triangle by taking its cross product
				// divided by 2.  However we can avoid dividing our area/base by 2
				// since our cross product will already be double our area.
				float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
				float wireThickness = 800 - _WireThickness;

				g2f o;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(i[0], o);
				o.worldPos = i[0].worldPos;
				o.pos = i[0].pos;
#ifdef _ALPHATEST_ON
				o.uv = i[0].uv;
#endif
				o.dist.xyz = float3((area / length(edge0)), 0.0, 0.0) * o.pos.w * wireThickness;
				o.dist.w = 1.0 / o.pos.w;
				triangleStream.Append(o);

				o.worldPos = i[1].worldPos;
				o.pos = i[1].pos;
				o.dist.xyz = float3(0.0, (area / length(edge1)), 0.0) * o.pos.w * wireThickness;
				o.dist.w = 1.0 / o.pos.w;
				triangleStream.Append(o);

				o.worldPos = i[2].worldPos;
				o.pos = i[2].pos;
				o.dist.xyz = float3(0.0, 0.0, (area / length(edge2))) * o.pos.w * wireThickness;
				o.dist.w = 1.0 / o.pos.w;
				triangleStream.Append(o);
			}

			half4 frag(g2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
#ifdef _ALPHATEST_ON
				half alpha = tex2D(_MainTex, i.uv).a;
				clip(alpha - _Cutoff);
#endif


				float minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];

				// Early out if we know we are not on a line segment.
				if (minDistanceToEdge > 0.9)
				{
#ifdef WIRE_ONLY
					discard;
#endif
					return fixed4(0,0,0,0);
				}

				// Smooth our line out
				float t = exp2(-2 * minDistanceToEdge * minDistanceToEdge);

#ifdef SPATIAL_COLOR
				const fixed4 colors[11] = {
						fixed4(1.0, 1.0, 1.0, 1.0),  // White
						fixed4(1.0, 0.0, 0.0, 1.0),  // Red
						fixed4(0.0, 1.0, 0.0, 1.0),  // Green
						fixed4(0.0, 0.0, 1.0, 1.0),  // Blue
						fixed4(1.0, 1.0, 0.0, 1.0),  // Yellow
						fixed4(0.0, 1.0, 1.0, 1.0),  // Cyan/Aqua
						fixed4(1.0, 0.0, 1.0, 1.0),  // Magenta
						fixed4(0.5, 0.0, 0.0, 1.0),  // Maroon
						fixed4(0.0, 0.5, 0.5, 1.0),  // Teal
						fixed4(1.0, 0.65, 0.0, 1.0), // Orange
						fixed4(1.0, 1.0, 1.0, 1.0)   // White
					};

				float cameraToVertexDistance = length(_WorldSpaceCameraPos - i.worldPos);
				int index = clamp(floor(cameraToVertexDistance), 0, 10);
				half4 wireColor = colors[index];
#else
				half4 wireColor = _Color;
#endif

				half4 finalColor = lerp(float4(0, 0, 0, 1), wireColor, t);
				finalColor.a = t;

				return finalColor;
			}
			ENDCG
		}
	}
}