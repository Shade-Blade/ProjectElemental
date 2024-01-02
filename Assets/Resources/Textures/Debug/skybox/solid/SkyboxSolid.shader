Shader "Custom/SkyboxSolid"
{
    Properties
    {
		[HDR]
		_SkyColor("Color", Color) = (0.37, 0.52, 0.73, 0)
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

			half4 _SkyColor;

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.position);
				o.texcoord = v.texcoord;
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				return _SkyColor;
			}
			ENDCG
		}
    }
    FallBack "Diffuse"
}
