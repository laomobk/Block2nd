Shader "B2nd/Block2nd_GUICube"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "RenderQueue"="5000"}
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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float lambert : TEXCOORD1;
			};

			sampler2D _TerrainTexture;
			float4 _TerrainTexture_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _TerrainTexture);
				o.lambert = clamp(dot(v.normal, normalize(float3(-1, 1, -1))), 0.8, 1);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_TerrainTexture, i.uv);
                if (col.a < 0.1)
                {
                    discard;
                }
				return fixed4(col.xyz * i.lambert, col.a);
			}
			ENDCG
		}
	}
}
