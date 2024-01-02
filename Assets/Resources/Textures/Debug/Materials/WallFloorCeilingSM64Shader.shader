Shader "Custom/WallFloorCeilingSM64Shader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _WallColor ("Z Axis Wall", Color) = (1,1,1,1)
        _AltWallColor ("X Axis Wall", Color) = (1,1,1,1)
        _FloorColor ("Floor", Color) = (1,1,1,1)
        _CeilingColor ("Ceiling", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;


        fixed4 _WallColor;
        fixed4 _AltWallColor;
        fixed4 _FloorColor;
        fixed4 _CeilingColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            //o.Albedo = c.rgb;
            
            //SM64 version

            //walls are triangles with normals between -0.01 and 0.01
            //so (vertically) sloped walls basically don't exist in SM64

            //Note that walls also project hitboxes in the X or Z directions only

            fixed4 _ShiftColor = fixed4(1,1,1,1);
            if (IN.worldNormal.y > 0.01) {
                _ShiftColor  = _FloorColor;
            } else if (IN.worldNormal.y > -0.01) {
                //not sure how it tie breaks 45 degree walls
                if (IN.worldNormal.x > IN.worldNormal.z) {
                   _ShiftColor  = _AltWallColor;
                } else {
                   _ShiftColor  = _WallColor;
                }
            } else {
                _ShiftColor  = _CeilingColor;
            }

            c *= _ShiftColor;
            o.Albedo = c;
            
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
