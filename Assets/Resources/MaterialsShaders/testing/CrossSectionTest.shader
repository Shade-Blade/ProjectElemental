Shader "Custom/Cross Section"{
    //show values to edit in inspector
    Properties{
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metalness", Range(0, 1)) = 0
        [HDR]_Emission ("Emission", color) = (0,0,0)

        //How to make a plane
        //  Plane plane = new Plane(transform.up, transform.position);
        //  Vector4 planeRepresentation = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
        _Plane ("Plane", Vector) = (0,0,0,0)

        [HDR]_CutoffColor("Cutoff Color", Color) = (1,0,0,0)
        _CutoffTex ("Cutoff Texture", 2D) = "white" {}
    }

    SubShader{
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "Queue" = "Transparent" "RenderType"="Transparent"}

        // render faces regardless if they point towards the camera or away from it
        Cull Off

        CGPROGRAM
        //the shader is a surface shader, meaning that it will be extended by unity in the background
        //to have fancy lighting and other features
        //our surface shader function is called surf and we use our custom lighting model
        //fullforwardshadows makes sure unity adds the shadow passes the shader might need
        //vertex:vert makes the shader use vert as a vertex shader function
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _CutoffTex;
        fixed4 _Color;

        half _Smoothness;
        half _Metallic;
        half3 _Emission;

        float4 _Plane;

        float4 _CutoffColor;

        //input struct which is automatically filled by unity
        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
            float facing : VFACE;
        };

        //the surface shader function which sets parameters the lighting function then uses
        void surf (Input i, inout SurfaceOutputStandard o) {
            //calculate signed distance to plane
            float distance = dot(i.worldPos, _Plane.xyz);
            distance = distance + _Plane.w;
            //discard surface above plane
            clip(-distance);

            float facing = i.facing * 0.5 + 0.5;

            //normal color stuff
            fixed4 col = tex2D(_MainTex, i.uv_MainTex);
            col *= _Color;
            o.Albedo = col.rgb * facing;
            o.Metallic = _Metallic * facing;
            o.Smoothness = _Smoothness * facing;

            if (facing == 0) {
                //How to calculate position onto the normal plane
                //get viewdir
                //walk backwards onto the plane
                //Note: at the intersection edge some points could be on the plane (in that case use worldpos)
                float den = dot(_Plane.xyz, -i.viewDir);
                float t = 0;
                //note 0 = parallel = intersect immediately
                if (den != 0) {
                    t = dot((float3(0,-(_Plane.w / _Plane.y),0) - i.worldPos), _Plane.xyz) / den;
                }

                //col = lerp(_Color, _CutoffColor, -t);

                float3 newPos = i.worldPos - i.viewDir * t;
                
                //col = lerp(_Color, _CutoffColor, 15 + newPos.z);

                //Need to transform these into 2d coordinates
                //for now just use xz planar mapping
                float2 newUV = float2(newPos.x, newPos.z);
                col = tex2D(_CutoffTex, newUV);
            }
            o.Emission = lerp(col, float4(0,0,0,0), facing);
        }
        ENDCG
    }
    FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}