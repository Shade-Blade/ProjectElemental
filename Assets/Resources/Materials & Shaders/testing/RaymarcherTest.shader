Shader "Custom/RaymarcherTest"
{
    Properties
    {
        [HDR]
        _Color ("Color", Color) = (1,1,1,1)
        _PlayerRadius("Player Radius", float) = 0.5
        _Centre("Center", Vector) = (0,0,0)
        _Radius("Sphere Radius", float) = 0.5
        _CentreB("Center B", Vector) = (0,0,0)
        _RadiusB("Sphere Radius B", float) = 0.5
        _CentreC("Center C", Vector) = (0,0,0)
        _RadiusC("Sphere Radius C", float) = 0.5
        _SpColor ("Specular Color", Color) = (0,0,0,0)
        _InColor ("Inside Color", Color) = (1,1,1,1)
        _AmColor ("Ambient Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        LOD 200

        Pass {
            CGPROGRAM

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma fade:alpha


            sampler2D _MainTex;

            struct v2f {
                float4 pos : SV_POSITION;    // Clip space
                float3 wPos : TEXCOORD1;    // World position
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;                
            };

            half4 _Color;
            fixed4 _SpColor;
            fixed4 _AmColor;
            float3 _Centre;
            float3 _CentreB;
            float3 _CentreC;
            float _Radius;
            float _RadiusB;
            float _RadiusC;

            uniform float4 _PlayerPos;
            float _PlayerRadius;
            fixed4 _InColor;


            #define STEPS 20
            #define MIN_DISTANCE 0.01

            #include "Lighting.cginc"
            fixed4 simpleLambert (float3 p, fixed3 normal, fixed3 viewDir) {

                fixed4 lightPos = _WorldSpaceLightPos0;
                fixed3 lightDir = lightPos.xyz;    // Light direction

                if (lightPos.w == 1) {
                    lightDir = normalize(lightPos.xyz - p);
                }

                fixed3 lightCol = _LightColor0.rgb;        // Light color

                half3 h = normalize (lightDir + viewDir);

                half diff = max (0, dot (normal, lightDir));

                float nh = max (0, dot (normal, h));
                float spec = pow (nh, 48.0);

                float atten = 1;
                if (_LightColor0.a == 0) {
                    atten = -atten;
                }

                half4 c;
                c.rgb = (_Color * _LightColor0.rgb * diff + _SpecColor * _LightColor0.rgb * spec) * atten + _AmColor;
                c.a = 1;
                return c;
            }

            float sphereDistance (float3 p)
            {
                return length(p - _Centre) - _Radius;
            }

            float sphereDistanceB (float3 p)
            {
                return length(p - _CentreB) - _RadiusB;
            }

            float sphereDistanceC (float3 p)
            {
                return length(p - _CentreC) - _RadiusC;
            }

            float spherePDistance (float3 p) {
                return length(p - _PlayerPos) - _PlayerRadius;
            }

            float smin( float a, float b, float k )
            {
                float h = max( k-abs(a-b), 0.0 )/k;
                return min( a, b ) - h*h*k*(1.0/4.0);
            }


            float combinedDistance(float3 p) {
                return smin(smin(smin(sphereDistance(p), sphereDistanceC(p), 1.5), sphereDistanceB(p), 1.5), spherePDistance(p), 1.5);
            }

            float map(float3 p) {
                return combinedDistance(p);
            }

            float3 normal (float3 p)
            {
                const float eps = 0.01;
                return normalize
                (    float3
                    (    map(p + float3(eps, 0, 0)    ) - map(p - float3(eps, 0, 0)),
                        map(p + float3(0, eps, 0)    ) - map(p - float3(0, eps, 0)),
                        map(p + float3(0, 0, eps)    ) - map(p - float3(0, 0, eps))
                    )
                );
            }

            fixed4 renderSurface(float3 p, float3 dir)
            {
                float3 n = normal(p);
                return simpleLambert(p, n, dir);
            }

            fixed4 raymarch (float3 position, float3 direction)
            {
                for (int i = 0; i < STEPS; i++)
                {
                    float distance = combinedDistance(position);
                    if (distance < MIN_DISTANCE) {
                        if (i == 0)
                            return _InColor;
                        return renderSurface(position, direction);
                    }
                    position += distance * direction;
                }
                return 0;
            }            


            // Vertex function
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz; 
                return o;
            }
            // Fragment function
            fixed4 frag (v2f i) : SV_Target
            {
                float3 worldPosition = i.wPos;
                float3 viewDirection = normalize(i.wPos - _WorldSpaceCameraPos);
                return raymarch (worldPosition, viewDirection);
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}
