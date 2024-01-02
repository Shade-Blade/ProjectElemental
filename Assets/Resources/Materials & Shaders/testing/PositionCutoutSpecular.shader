Shader "Custom/PositionCutoutSpecular"
{
    Properties
    {
        _Color ("Color A", Color) = (1,1,1,1)
        _ColorB ("Color B", Color) = (1,1,1,1)
        _SpecColor ("Specular Color", Color) = (0,0,0,0)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _FadeLength ("Fade Length", float) = 0.0
        _Distance ("Distance", float) = 1
        _WorldOffset ("Behind Offset", float) = 1
        _NearOffset ("Near Camera Length", float) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

        Cull Off
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf SimpleSpecular fullforwardshadows alpha:fade

        //half4 _SpecColor;

        half4 LightingSimpleSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
            half3 h = normalize (lightDir + viewDir);

            half diff = max (0, dot (s.Normal, lightDir));

            float nh = max (0, dot (s.Normal, h));
            float spec = pow (nh, 48.0);

            if (_LightColor0.a == 0) {
                atten = -atten;
            }

            half4 c;
            c.rgb = (s.Albedo * _LightColor0.rgb * diff + _SpecColor * _LightColor0.rgb * spec) * atten;
            c.a = s.Alpha;
            return c;
        }

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

        void surf (Input IN, inout SurfaceOutput o)
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
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
