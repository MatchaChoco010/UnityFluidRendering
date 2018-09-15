Shader "GBuffer/GBufferSphere"
{
	Properties
	{
		_GBuffer0Color ("GBuffer0 Color", Color) = (0, 0, 0, 0)
    _GBuffer1Color ("GBuffer1 Color", Color) = (0, 0, 0, 0)
    _GBuffer3Color ("GBuffer3 Color", Color) = (0, 0, 0, 0)
		_Radius ("Radius", float) = 1.0
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
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 eyeSpacePos : TEXCOORD1;
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
			uniform float _Radius;

			void vert (in appdata v, out v2f o)
			{
				o.position = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.uv = v.uv;
				o.eyeSpacePos = UnityObjectToViewPos(v.vertex);
			}

			void frag (in v2f i, out flagout o)
			{
				float3 clipSpaceNormal;
				clipSpaceNormal.xy = i.uv *2 - 1;
				float r2 = dot(clipSpaceNormal.xy, clipSpaceNormal.xy);
				if (r2 > 1.0) discard;
				clipSpaceNormal.z = sqrt(1.0 - r2);

				float4 pixelPos = float4(i.eyeSpacePos + clipSpaceNormal * _Radius, 1);
				float4 clipSpacePos = mul(UNITY_MATRIX_P, pixelPos);
				o.depth = clipSpacePos.z / clipSpacePos.w;

				float4 worldSpaceNormal = mul(
					transpose(UNITY_MATRIX_V),
					float4(clipSpaceNormal.xyz, 0)
				);

				// o.gBuffer0 = float4(i.uv, 0, 1);
				o.gBuffer0 = _GBuffer0Color;
				o.gBuffer1 = _GBuffer1Color;
				// o.gBuffer2 = float4(i.normal, 0) * 0.5 + float4(0.5, 0.5, 0.5, 0);
				o.gBuffer2 = worldSpaceNormal * 0.5 + float4(0.5, 0.5, 0.5, 0);
				o.gBuffer3 = _GBuffer3Color;
			}
			ENDCG
		}
	}
}
