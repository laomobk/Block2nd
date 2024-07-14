﻿Shader "B2nd/Block2nd_HoldCube"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		
		Blend SrcAlpha OneMinusSrcAlpha
		

		Pass
		{
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _EnvLight;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.lambert = clamp(dot(UnityObjectToWorldNormal(v.normal), 
								normalize(WorldSpaceLightDir(v.vertex))) * 0.5 + 0.5, 0.5, 1) + 0.1;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
                if (col.a < 0.1)
                {
                    discard;
                }
				return fixed4(col.xyz * i.lambert * (_EnvLight * 0.5 + 0.5), col.a);
			}
			ENDCG
		}
	}
}
