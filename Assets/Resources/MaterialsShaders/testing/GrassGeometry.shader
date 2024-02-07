//Forgot where I got this from
//It had a tesselation thing earlier

Shader "Geometry/GrassGeometryShaderUnlit"
{
    Properties
    {
        //Color stuff
        [HDR]
        _ColorLow("Color Low", Color) = (1,1,1,1)
        [HDR]
        _ColorHigh("Color High", Color) = (1,1,1,1)
        [HDR]
        _ColorLowWind("Color Low Wind", Color) = (1,1,1,1)
        [HDR]
        _ColorHighWind("Color High Wind", Color) = (1,1,1,1)
        //_Color("Color", Color) = (1,1,1,1)
        //_GradientMap("Gradient map", 2D) = "white" {}
         
        //Noise and wind
        _NoiseTexture("Noise texture", 2D) = "white" {} 
        _WindTexture("Wind texture", 2D) = "white" {}
        _WindStrength("Wind strength", float) = 0
        _WindSpeed("Wind speed", float) = 0
        //[HDR]
        //_WindColor("Wind color", Color) = (1,1,1,1)
 
        //Position and dimensions
        _GrassHeight("Grass height", float) = 0
        _GrassWidth("Grass width", Range(0.0, 1.0)) = 1.0
        _PositionRandomness("Position randomness", float) = 0

        _PlayerPush("Player Push", float) = 0
        _PlayerDist("Player Dist", float) = 0
 
        //Grass blades
        _GrassBlades("Grass blades per triangle", Range(0, 13)) = 1
        _MinimunGrassBlades("Minimum grass blades per triangle", Range(0, 13)) = 1
        _MaxCameraDistance("Max camera distance", float) = 10
        
        _TranslucentGain("Translucent gain", float) = 0.25

        //Light stuff
        [Toggle(IS_LIT)]
        _IsLit("Is lit", float) = 0
        _RimPower("Rim power", float) = 1
        [HDR]_TranslucentColor("Translucent color", Color) = (1,1,1,1)

        _TessellationGrassDistance("Grass Distance", float) = 1
    }
    SubShader
    {
 
        CGINCLUDE
         
            #include "UnityCG.cginc"
            #include "Autolight.cginc"
 
            /*
            struct appdata
            {
                float4 vertex : POSITION;
            };
 
            struct v2g
            {
                float4 vertex : POSITION;
            };
            */

            struct VertexInput
            {
                 float4 vertex  : POSITION;
                 float3 normal  : NORMAL;
                 float4 tangent : TANGENT;
                 float2 uv      : TEXCOORD0;
            };

            struct VertexOutput
            {
                 float4 vertex  : SV_POSITION;
                 float3 normal  : NORMAL;
                 float4 tangent : TANGENT;
                 float2 uv      : TEXCOORD0;
            };

 
            struct g2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 col : COLOR;
                float3 normal : NORMAL;
                unityShadowCoord4 _ShadowCoord : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float3 vertexLighting : TEXCOORD3;
            };
 

            uniform float4 _PlayerPos;

            half4 _ColorLow;
            half4 _ColorHigh;
            half4 _ColorLowWind;
            half4 _ColorHighWind;
            //sampler2D _GradientMap;
 
            sampler2D _NoiseTexture;
            float4 _NoiseTexture_ST;
            sampler2D _WindTexture;
            float4 _WindTexture_ST;
            float _WindStrength;
            float _WindSpeed;
            half4 _WindColor;
 
            float _GrassHeight;
            float _GrassWidth;
            float _PositionRandomness;
            float _PlayerPush;
            float _PlayerDist;
 
            float _GrassBlades;
            float _MinimunGrassBlades;
            float _MaxCameraDistance;

            float _TranslucentGain;
 
            float _TessellationGrassDistance;

            float random (float2 st) {
                return frac(sin(dot(st.xy,
                                    float2(12.9898,78.233)))*
                    43758.5453123);
            }
 
 
            g2f GetVertex(float4 pos, float2 uv, fixed4 col, float3 normal) {
                g2f o;
                o.vertex = UnityObjectToClipPos(pos);
                o.uv = uv;
                o.viewDir = WorldSpaceViewDir(pos);
                o.col = col;
                o._ShadowCoord = ComputeScreenPos(o.vertex);
                o.normal = UnityObjectToWorldNormal(normal);
                #if UNITY_PASS_SHADOWCASTER
                o.vertex = UnityApplyLinearShadowBias(o.vertex);
                #endif

                float3 world = mul(unity_ObjectToWorld, pos);

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
 

            
            VertexOutput tessVert(VertexInput v)
            {
                 VertexOutput o;
                 o.vertex = v.vertex;
                 o.normal = v.normal;
                 o.tangent = v.tangent;
                 o.uv = v.uv;
                 return o;
            }

            /*
            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
                o.vertex = v.vertex;
                return o;
            }
            */


            //new tesselation stuff
            struct TessellationFactors
            {
                 float edge[3] : SV_TessFactor;
                 float inside  : SV_InsideTessFactor;
            };

            float tessellationEdgeFactor (float3 p0, float3 p1) {
		        float edgeLength = distance(p0, p1);

		        float3 edgeCenter = (p0 + p1) * 0.5;
		        float viewDistance = distance(edgeCenter, _WorldSpaceCameraPos);

		        return edgeLength /
			        (max(_TessellationGrassDistance, 0.2));
            }

            TessellationFactors MyPatchConstantFunction (
	            InputPatch<VertexInput, 3> patch
            ) {
	            float3 p0 = mul(unity_ObjectToWorld, patch[0].vertex).xyz;
	            float3 p1 = mul(unity_ObjectToWorld, patch[1].vertex).xyz;
	            float3 p2 = mul(unity_ObjectToWorld, patch[2].vertex).xyz;
	            TessellationFactors f;
                f.edge[0] = tessellationEdgeFactor(p1, p2);
                f.edge[1] = tessellationEdgeFactor(p2, p0);
                f.edge[2] = tessellationEdgeFactor(p0, p1);
	            f.inside =
		            (tessellationEdgeFactor(p1, p2) +
		            tessellationEdgeFactor(p2, p0) +
		            tessellationEdgeFactor(p0, p1)) * (1 / 3.0);
	            return f;
            }
            
            [domain("tri")]
            [outputcontrolpoints(3)]
            [outputtopology("triangle_cw")]
            [partitioning("integer")]
            [patchconstantfunc("MyPatchConstantFunction")]
            VertexInput hull(InputPatch<VertexInput, 3> patch, uint id : SV_OutputControlPointID)
            {
                 return patch[id];
            }

            [domain("tri")]
            VertexOutput domain(TessellationFactors factors, OutputPatch<VertexInput, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                 VertexInput i;

                 // Create interpolation macro.
                 #define INTERPOLATE(fieldname) i.fieldname = \
                      patch[0].fieldname * barycentricCoordinates.x + \
                      patch[1].fieldname * barycentricCoordinates.y + \
                      patch[2].fieldname * barycentricCoordinates.z;

                 INTERPOLATE(vertex)
                 INTERPOLATE(normal)
                 INTERPOLATE(tangent)
                 INTERPOLATE(uv)

                 return tessVert(i);
            }



 
            //3 + 3 * 13 = 42
            [maxvertexcount(42)]    //unity gets mad if this is higher, something about (this) * 20 > 1024
            void geom(triangle VertexOutput input[3], inout TriangleStream<g2f> triStream)
            {
                float3 normal = normalize(cross(input[1].vertex - input[0].vertex, input[2].vertex - input[0].vertex));
                int grassBlades = ceil(lerp(_GrassBlades, _MinimunGrassBlades, saturate(distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, input[0].vertex)) / _MaxCameraDistance)));

                //Use triangle area to scale how many grass blades there should be
                grassBlades = ceil(grassBlades * ((length(cross(input[0].vertex - input[1].vertex, input[0].vertex - input[2].vertex))) / (max(_TessellationGrassDistance * _TessellationGrassDistance, 0.2 * 0.2))));

                grassBlades = clamp(grassBlades, 1, 15);

                for (uint i = 0; i < grassBlades; i++) {
                    float r1 = random(mul(unity_ObjectToWorld, input[0].vertex).xz * (i + 1));
                    float r2 = random(mul(unity_ObjectToWorld, input[1].vertex).xz * (i + 1));
 
                    //Random barycentric coordinates from https://stackoverflow.com/a/19654424
                    float4 midpoint = (1 - sqrt(r1)) * input[0].vertex + (sqrt(r1) * (1 - r2)) * input[1].vertex + (sqrt(r1) * r2) * input[2].vertex;
 
                    r1 = r1 * 2.0 - 1.0;
                    r2 = r2 * 2.0 - 1.0;
 
                    float4 pointA = midpoint + _GrassWidth * normalize(input[i % 3].vertex - midpoint);
                    float4 pointB = midpoint - _GrassWidth * normalize(input[i % 3].vertex - midpoint);
 
                    float4 worldPos = mul(unity_ObjectToWorld, midpoint);
 
                    float2 windTex = tex2Dlod(_WindTexture, float4(worldPos.xz * _WindTexture_ST.xy + _Time.y * _WindSpeed, 0.0, 0.0)).xy;
                    float2 wind = (windTex * 2.0 - 1.0) * _WindStrength;
 
                    float noise = tex2Dlod(_NoiseTexture, float4(worldPos.xz * _NoiseTexture_ST.xy, 0.0, 0.0)).x;
                    float heightFactor = noise * _GrassHeight;                        
 
                    triStream.Append(GetVertex(pointA, float2(0,0), fixed4(0,0,0,1), normal));
 
                    float playerDist = length(worldPos - _PlayerPos);
                    float4 diff = worldPos - _PlayerPos;
                    diff.y = 0;
                    diff.w = 0;
                    diff = normalize(diff);

                    float offset = (_PlayerDist / (playerDist + 0.1)) - 1;

                    offset = clamp(offset, 0, 5);

                    diff *= offset * _PlayerPush;

                    float4 newVertexPoint = midpoint + diff + float4(normal, 0.0) * heightFactor + float4(r1, 0.0, r2, 0.0) * _PositionRandomness + float4(wind.x, 0.0, wind.y, 0.0);
                    float3 bladeNormal = normalize(cross(pointB.xyz - pointA.xyz, midpoint.xyz - newVertexPoint.xyz));
                    triStream.Append(GetVertex(newVertexPoint, float2(0.5, 1), fixed4(1.0, length(windTex), 1.0, 1.0), bladeNormal));
 
                    triStream.Append(GetVertex(pointB, float2(1,0), fixed4(0,0,0,1), normal));
 
                    triStream.RestartStrip();
                }
                 
 
                for (int i = 0; i < 3; i++) {
                    triStream.Append(GetVertex(input[i].vertex, float2(0,0), fixed4(0,0,0,1), normal));
                }
 
 
                triStream.RestartStrip();
            }
 
            /*
            fixed4 frag (g2f i) : SV_Target
            {
                fixed4 gradientMapCol = tex2D(_GradientMap, float2(i.col.x, 0.0));
                fixed4 col = (gradientMapCol + _WindColor * i.col.g) * _Color;
                return col;
            }
            */
             
 
        ENDCG
 
        Pass
        {
            Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
            Cull Off
            CGPROGRAM
            #pragma vertex tessVert
            #pragma geometry geom
            #pragma fragment frag
            #pragma hull hull
            #pragma domain domain
            #pragma target 4.6
            #pragma multi_compile_fwdbase
            #pragma shader_feature IS_LIT
             
            #include "Lighting.cginc"
                         
            float _RimPower;
            half4 _TranslucentColor;
 
            half4 frag (g2f i) : SV_Target
            {
                i.col.x = saturate(i.col.x);
                i.col.y = saturate(i.col.y);

                half4 vertColorA = lerp(_ColorLow, _ColorHigh, i.col.x);
                half4 vertColorB = lerp(_ColorLowWind, _ColorHighWind, i.col.x);

                //half4 gradientMapCol = tex2D(_GradientMap, float2(i.col.x, 0.0));
                //half4 col =  (gradientMapCol + _WindColor * i.col.y) * _Color;

                half4 col = lerp(vertColorA, vertColorB, i.col.y);

                #ifdef IS_LIT
                float light = saturate(dot(normalize(_WorldSpaceLightPos0), i.normal)) * 0.5 + 0.5;
                half4 translucency = _TranslucentColor * saturate(dot(normalize(-_WorldSpaceLightPos0), normalize(i.viewDir)));
                half rim =  pow(1.0 - saturate(dot(normalize(i.viewDir), i.normal)), _RimPower);
                float shadow = SHADOW_ATTENUATION(i);
                col *= (light + translucency * rim * i.col.x ) * _LightColor0 * shadow + float4( ShadeSH9(float4(i.normal, 1)), 1.0) ;
                //col *= (translucency * rim * i.col.x ) * _LightColor0 * shadow + float4( ShadeSH9(float4(i.normal, 1)), 1.0) ;
                col += half4(i.vertexLighting, 1);
                #endif 
                return col;
            }
             
            ENDCG
        }
 
        Pass
        {
            Tags {
                "LightMode" = "ShadowCaster"
            }
            CGPROGRAM
            #pragma vertex tessVert
            #pragma geometry geom
            #pragma fragment fragShadow
            #pragma hull hull
            #pragma domain domain
 
            #pragma target 4.6
            #pragma multi_compile_shadowcaster
 
            float4 fragShadow(g2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }            
             
            ENDCG
        }
 
    }
}