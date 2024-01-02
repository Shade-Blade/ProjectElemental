Shader "Custom/SkyboxGradient"
{
    Properties
    {
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
    }
    SubShader
    {
        Tags{ "RenderType" = "Opaque" "Queue" = "Background" }
		ZWrite Off
		Fog{ Mode Off }
		Cull Off

        Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct appdata
			{
				float4 position : POSITION;
				float3 texcoord : TEXCOORD0;
			};
			struct v2f
			{
				float4 position : SV_POSITION;
				float3 texcoord : TEXCOORD0;
			};

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

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.position);
				o.texcoord = v.texcoord;
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half3 v = normalize(i.texcoord);

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

				if (dotVal > _SunRim)
					return _SunColor;
				else
					return outColor;
			}
			ENDCG
		}
    }
    FallBack "Diffuse"
}
