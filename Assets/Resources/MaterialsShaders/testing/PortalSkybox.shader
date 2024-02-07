Shader "Custom/PortalSky" {    //make this cone shaped
    //show values to edit in inspector
    Properties {
		[HDR]
		_SkyColor1("Top Color", Color) = (0.37, 0.52, 0.73, 0)
		[HDR]
		_SkyColor2("2nd color", Color) = (0.89, 0.96, 1, 0)
		[HDR]
		_SkyColor3("3rd color", Color) = (0.2 ,0.4 ,0.6 , 0)
		[HDR]
		_SkyColor4("4th color", Color) = (0.2 ,0.4 ,0.6 , 0)
		[HDR]
		_SkyColor5("5th color", Color) = (0.2 ,0.4 ,0.6 , 0)
		[HDR]
		_SkyColor6("Bottom color",Color) = (0.4, 0.2, 0.1, 0)
		_PointA("Point A", float) = 0.9
		_PointB("Point B", float) = 0.9
		_MountainAmount("Mountains", float) = 0
		_MountainPoint("Mountain Point", float) = 0
		_MountainHeight("Mountain Height", float) = 0
		[HDR]
		_SunColor("Sun color",Color) = (0.4, 0.2, 0.1, 0)
		_SunRim("Sun Rim", float) = 0.9
		_LuminosityFix("LuminosityFix", float) = 0.6666	//hacky fix (might be an ambient light thing? other lights are ignored correctly) (note: make sure the mesh has no shadow casting)
		[IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
    }
    SubShader {
		//note: unity treats 1000 queue as skybox and not something you can put on stuff
		//	so change it to something higher then fix it later
		//	1001 queue is probably better?
		//	this is a skybox replacer so 2000 or higher queue is wrong (this is opaque and has to render below stuff)

		//Note: This shader does not work because it gets clipped
		//ZWrite On will fix it but also occlude everything behind it (not the desired effect)
		//Probably the best way to reproduce this effect is to just turn on Zwrite and make it only appear on the furthest back stuff

		//alternatively: add a depth check so that it only replaces sky (better idea? overdraw is bad though)

        Tags{ "RenderType"="Transparent" "Queue"="Transparent"}
        
        //Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On
		Cull Front	//cull off creates wrong ish results (lighting is messing up somehow, even though there should be no lighting)

		//for rendering / not rendering things in the influence of the portal
		//stencil operation
		/*
		Stencil{
			Ref [_StencilRef]
			Comp Always
			ZFail Keep
			Pass Replace
		}
		*/


        CGPROGRAM
        

        //because all surface shaders need a lighting model
        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
    	    fixed4 c;
    	    c.rgb = s.Albedo; 
    	    c.a = s.Alpha;
            return half4(0,0,0,1); //c;
        }

        //the shader is a surface shader, meaning that it will be extended by unity in the background to have fancy lighting and other features
        //our surface shader function is called surf and we use the standard lighting model, which means PBR lighting
        //fullforwardshadows makes sure unity adds the shadow passes the shader might need
        #pragma surface surf NoLighting noforwardadd
        #pragma target 3.0

		half4 _SkyColor1;
		half4 _SkyColor2;
		half4 _SkyColor3;
		half4 _SkyColor4;
		half4 _SkyColor5;
		half4 _SkyColor6;
		half4 _SunColor;
		half _PointA;
		half _PointB;
		half _MountainAmount;
		half _MountainPoint;
		half _MountainHeight;
		half _SunRim;

		half _LuminosityFix;

        //input struct which is automatically filled by unity
        struct Input
        {
            float3 viewDir;
        };

        //the surface shader function which sets parameters the lighting function then uses
        void surf (Input i, inout SurfaceOutput o) {
            half3 v = normalize(-i.viewDir);

			half dotVal = dot(v, _WorldSpaceLightPos0.xyz);

			half vertDot = dot(v, float3(0,1,0));

			v.y = 0;
			v = normalize(v);
			half horiDot = dot(v, float3(1,0,0));

			half atanRes = asin(horiDot);
			atanRes /= 3.141592653589793238462;	//probably enough digits to reach maximum precision
			atanRes += 0.5;
			atanRes /= 2;

			atanRes *= _MountainAmount;

			//triangle wave
			half sawtooth = 2.0 * abs( 2 * (atanRes - floor(0.5 + atanRes)) ) - 1.0;

			sawtooth *= _MountainHeight;

			fixed4 outColor;

			half mountainMod = (_MountainPoint + 1) / 2;

			//[-1, 1] -> [0, 1]
			half modified = (vertDot + 1) / 2;

			//do the same to all the points used
			half pointBMod = (_PointB + 1) / 2;
			half pointAMod = (_PointA + 1) / 2;

			if (modified > pointBMod) {
				outColor = lerp(_SkyColor2, _SkyColor1, (modified - pointBMod) / (1 - pointBMod));
			} else if (modified > pointAMod) {
				if (modified > mountainMod + sawtooth) {
					outColor = _SkyColor3;
				} else {
					outColor = _SkyColor4;
				}
				//outColor = lerp(_SkyColor4, _SkyColor3, (modified - pointAMod) / (pointBMod - pointAMod));
			} else {
				outColor = lerp(_SkyColor6, _SkyColor5, (modified) / (pointAMod));
			}

			o.Alpha = 1;
			if (dotVal > _SunRim)
				o.Albedo = _SunColor * _LuminosityFix;
				//return _SunColor;
			else
				o.Albedo = outColor * _LuminosityFix;
				//return outColor;
        }
        ENDCG
    }
    //FallBack "Standard"
}