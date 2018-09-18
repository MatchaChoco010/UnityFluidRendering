Shader "Fluid/FluidParticle"
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
			Name "Instancing"
			Stencil {
				Ref 129
				WriteMask 129
				Pass Replace
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 eyeSpacePos : TEXCOORD1;
			};

			struct flagout
			{
				float depth: SV_Target;
			};

			uniform float _Radius;

			void vert (in appdata v, out v2f o)
			{

				UNITY_SETUP_INSTANCE_ID (v);

				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.eyeSpacePos = UnityObjectToViewPos(v.vertex);
			}

			void frag (in v2f i, out flagout o)
			{
				float3 normal;
				normal.xy = i.uv *2 - 1;
				float r2 = dot(normal.xy, normal.xy);
				if (r2 > 1.0) discard;
				normal.z = sqrt(1.0 - r2);

				float4 pixelPos = float4(i.eyeSpacePos + normal * _Radius, 1);
				float4 clipSpacePos = mul(UNITY_MATRIX_P, pixelPos);
				o.depth = Linear01Depth(clipSpacePos.z / clipSpacePos.w);
			}
			ENDCG
		}

		CGINCLUDE
		#include "UnityCG.cginc"

		float bilateralBlur(float2 uv, sampler2D depthSampler, float2 blurDir) {
			float depth = tex2D(depthSampler, uv).x;

			float radius = min(1 / Linear01Depth(depth), 50);

			float sum = 0;
			float wsum = 0;

			for (float x = -radius; x <= radius; x += 1) {
				float sample = tex2Dlod(depthSampler, float4(uv + x * blurDir, 0, 0)).x;

				float r = x * 0.2;
				float w = exp(-r * r);

				float r2 = (sample - depth) * 5;
				float g = exp(-r2*r2);

				sum += sample * w * g;
				wsum += w * g;
			}

			if (wsum > 0) {
				sum /= wsum;
			}

			return sum;
		}
		ENDCG

		Pass {
			Name "xBlur"
			ZTest Always
			Blend One Zero
			Stencil {
				Ref 1
				ReadMask 1
				Comp Equal
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			struct flagout
			{
				float depth : SV_TARGET;
			};

			uniform sampler2D _Depth0RT;

			void vert (in appdata v, out v2f o)
			{
				o.position = v.vertex;
				o.uv = v.uv;
			}

			void frag (in v2f i, out flagout o)
			{
				o.depth = bilateralBlur(
					i.uv,
					_Depth0RT,
					float2(1 / _ScreenParams.x, 0)
				);
			}
			ENDCG
		}

		Pass {
			Name "yBlur"
			ZTest Always
			Blend One Zero
			Stencil {
				Ref 1
				ReadMask 1
				Comp Equal
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			struct flagout
			{
				float depth : SV_TARGET;
			};

			uniform sampler2D _Depth1RT;

			void vert (in appdata v, out v2f o)
			{
				o.position = v.vertex;
				o.uv = v.uv;
			}

			void frag (in v2f i, out flagout o)
			{
				o.depth = bilateralBlur(
					i.uv,
					_Depth1RT,
					float2(0, 1 / _ScreenParams.y)
				);
			}
			ENDCG
		}

		Pass {
			Name "CalculateNormal"
			Blend One Zero
			Stencil {
				Ref 1
				ReadMask 1
				Comp Equal
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
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
			uniform sampler2D _Depth0RT;
			uniform float4 _FrustumCorner;

			void vert (in appdata v, out v2f o)
			{
				o.position = v.vertex;
				o.uv = v.uv;
			}

			float3 uvToEyeSpacePos(float2 uv, sampler2D depth)
			{
				float d = tex2D(depth, uv).x;
				float3 frustumRay = float3(
					lerp(_FrustumCorner.x, _FrustumCorner.y, uv.x),
					lerp(_FrustumCorner.z, _FrustumCorner.w, uv.y),
					_ProjectionParams.z
				);
				return frustumRay * d;
			}

			void frag (in v2f i, out flagout o)
			{
				float3 eyeSpacePos = uvToEyeSpacePos(i.uv, _Depth0RT);
				o.depth = mul(UNITY_MATRIX_P, float4(eyeSpacePos, 1)).z;

				float3 ddx = uvToEyeSpacePos(i.uv + float2(1 / _ScreenParams.x, 0), _Depth0RT) - eyeSpacePos;
				float3 ddx2 = eyeSpacePos - uvToEyeSpacePos(i.uv - float2(1 / _ScreenParams.x, 0), _Depth0RT);
				if (abs(ddx.z) > abs(ddx2.z)) {
					ddx = ddx2;
				}

				float3 ddy = uvToEyeSpacePos(i.uv + float2(0, 1 / _ScreenParams.y), _Depth0RT) - eyeSpacePos;
				float3 ddy2 = eyeSpacePos - uvToEyeSpacePos(i.uv - float2(0, 1 / _ScreenParams.y), _Depth0RT);
				if (abs(ddy2.z) < abs(ddy.z)) {
					ddy = ddy2;
				}

				float3 normal = cross(ddy, ddx);
				normal = normalize(normal);
				#if defined(UNITY_REVERSED_Z)
					normal.z = -normal.z;
				#endif

				float4 worldSpacewNormal = mul(
					transpose(UNITY_MATRIX_V),
					float4(normal, 0)
				);

				o.gBuffer0 = _GBuffer0Color;
				o.gBuffer1 = _GBuffer1Color;
				o.gBuffer2 = float4(worldSpacewNormal * 0.5 + float3(0.5, 0.5, 0.5), 1);
				o.gBuffer3 = _GBuffer3Color;
			}
			ENDCG
		}
	}
}
