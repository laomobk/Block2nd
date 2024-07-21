Shader "B2nd/Block2nd_HoldCube"
{
	Properties
	{
		
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		
		Blend SrcAlpha OneMinusSrcAlpha
		

		Pass
		{
			Cull off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float3 light : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float lambert : TEXCOORD1;
				float3 light : TEXCOORD2;
			};

			sampler2D _TerrainTexture;
			float4 _TerrainTexture_ST;
			float4 _EnvLight;
			
			fixed4 _SkyLightColor;
			fixed4 _BlockLightColor;
			fixed4 _SkyHorizonColor;
			
			float _SkyLightLuminance;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _TerrainTexture);
				o.lambert = clamp(dot(UnityObjectToWorldNormal(v.normal), 
								normalize(WorldSpaceLightDir(v.vertex))) * 0.5 + 0.5, 0.5, 1) + 0.1;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texColor = tex2D(_TerrainTexture, i.uv);

				if (texColor.a == 0)
					discard;
				
				fixed4 skyLight = max(0.35, _EnvLight.r) * _SkyLightColor;
				fixed4 blockLight = max(0.35, _EnvLight.g) * _BlockLightColor;

				float blendFactor = min(_EnvLight.r, _SkyLightLuminance);
				fixed4 blendedLight = ((blendFactor) * skyLight + (1 - blendFactor) * blockLight);

				fixed3 col = saturate(texColor.xyz * i.lambert * blendedLight);

				return fixed4(col, texColor.a);
			}
			ENDCG
		}
	}
}
