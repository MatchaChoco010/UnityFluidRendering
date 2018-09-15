Shader "GBuffer/GBufferTest"
{
	Properties
	{
		_GBuffer0Color ("GBuffer0 Color", Color) = (0, 0, 0, 0)
    _GBuffer1Color ("GBuffer1 Color", Color) = (0, 0, 0, 0)
    _GBuffer3Color ("GBuffer3 Color", Color) = (0, 0, 0, 0)
	}
	SubShader
	{
		Pass
		{
      Stencil
      {
        Comp Always
        Pass Replace
        Ref 128
      }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float3 normal : NORMAL;
			};

			struct flagout
			{
				float4 gBuffer0 : SV_TARGET0;
				float4 gBuffer1 : SV_TARGET1;
				float4 gBuffer2 : SV_TARGET2;
				float4 gBuffer3 : SV_TARGET3;
				float depth: SV_DEPTH;
			};

			uniform float4 _GBuffer0Color;
			uniform float4 _GBuffer1Color;
			uniform float4 _GBuffer3Color;

			void vert (in appdata v, out v2f o)
			{
				o.position = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
			}

			void frag (in v2f i, out flagout o)
			{
				o.gBuffer0 = _GBuffer0Color;
				o.gBuffer1 = _GBuffer1Color;
				o.gBuffer2 = float4(i.normal, 0) * 0.5 + float4(0.5, 0.5, 0.5, 0);
				o.gBuffer3 = _GBuffer3Color;
				o.depth = i.position.z;
			}
			ENDCG
		}
	}
}
