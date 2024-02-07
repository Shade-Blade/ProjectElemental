Shader "Custom/PositionCutoutB"
{
    Properties
    {
        _Color ("Color A", Color) = (1,1,1,1)
        _ColorB ("Color B", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _FadeLength ("Fade Length", float) = 0.0
        _Distance ("Distance", float) = 1
        _WorldOffset ("Behind Offset", float) = 1
        _NearOffset ("Near Camera Length", float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="False" "RenderType"="Transparent" }

        Cull Off
        //Blend SrcAlpha OneMinusSrcAlpha
        LOD 200


        //special weird thing that makes the transparency artifacts disappear for some reason
        //first pass renders A channel
        //though this means I have to do the alpha calculation beforehand
        Pass {
			ColorMask A
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            
			#include "UnityCG.cginc"
			fixed4 _Color;

            sampler2D _MainTex;
			float4 _MainTex_ST;

            struct vertInput {
				float4 vertex:POSITION;
				float3 normal:NORMAL;
                float2 uv : TEXCOORD0;
			};

			struct fragInput {
				float4 pos:SV_POSITION;
                float2 uv : TEXCOORD0;
			};

			fragInput vert(vertInput i) {
				fragInput o;
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				o.pos = UnityObjectToClipPos(i.vertex);
				return o;
			}

			half4 frag(fragInput fi) : SV_Target {
				half4 col = tex2D(_MainTex, fi.uv) * _Color;

                return col;
			}

			ENDCG
		}

        
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _ColorB;

        float _FadeLength;
        float _Distance;
        float _WorldOffset;
        float _NearOffset;
        
		uniform float4 _PlayerPos;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c;
            fixed4 cn = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 ca = cn * _Color;
            fixed4 cb = cn * _ColorB;

            float3 camVector = _WorldSpaceCameraPos - _PlayerPos;
            float3 viewVector = IN.worldPos - _PlayerPos;

            float camLength = length(camVector);
            camVector = camVector / camLength;
            float viewLength = length(viewVector);
            viewVector = viewVector / viewLength;


            float dotc = dot(camVector, viewVector);

            float cylinderFactor = viewLength * (sqrt(1 - (dotc * dotc)));

            //Calculate the cylinder value

            //c dot v = sin(theta)
            //|v| sin(theta) = cylindrical distance

            float delta = -cylinderFactor;

            float worldLength = length(_WorldSpaceCameraPos - IN.worldPos);

            //don't cut out behind the player character
            float altDeltaFactor = (camLength - worldLength + _WorldOffset) / _FadeLength;

            delta = min(altDeltaFactor, delta);

            //if the object is right up next to the camera, make it fade out so that it doesn't just pop in
            float nearDelta = _NearOffset * worldLength;

            nearDelta = saturate(nearDelta);

            //Which texture to use?
            float interp;

            if (_FadeLength == 0) {
                delta = (delta + _Distance) > 0 ? 1 : -1;
            } else {
                delta = (delta + _Distance) / _FadeLength;
            }

            //[-1, 1] -> [0, 1]
            delta = saturate((delta + 1) / 2);
            interp = delta;

            c = lerp(ca, cb, interp);

            c.a *= nearDelta;

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
