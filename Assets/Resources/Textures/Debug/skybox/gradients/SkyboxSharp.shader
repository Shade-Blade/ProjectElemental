Shader "Custom/SkyboxGradient"
{
    Properties
    {
		_SkyColor1("Top Color", Color) = (0.37, 0.52, 0.73, 0)
		_SkyColor2("2nd color", Color) = (0.89, 0.96, 1, 0)
		_SkyColor3("Bottom Color", Color) = (0.2 ,0.4 ,0.6 , 0)
		_PointA("Point A", float) = 0.9
		_PointB("Point B", float) = 0.9
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

			fixed4 _SkyColor1;
			fixed4 _SkyColor2;
			fixed4 _SkyColor3;
			fixed4 _SunColor;
			half _PointA;
			half _PointB;
			half _SunRim;

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.position);
				o.texcoord = v.texcoord;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				half3 v = normalize(i.texcoord);

				half dotVal = dot(v, _WorldSpaceLightPos0.xyz);

				half vertDot = dot(v, float3(0,1,0));

				fixed4 outColor;

				//[-1, 1] -> [0, 1]
				half modified = (vertDot + 1) / 2;

				//do the same to all the points used
				half pointBMod = (_PointB + 1) / 2;
				half pointAMod = (_PointA + 1) / 2;

				if (modified > pointBMod) {
					outColor = _SkyColor1;
				} else if (modified > pointAMod) {
					outColor = _SkyColor2;
				} else {
					outColor = _SkyColor3;
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
