﻿Shader "B2nd/Block2nd_SelectBox"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		
		_BorderTex ("BorderTexture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		// ZTest Always
		Offset -1, -1
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		    Cull off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			sampler2D _BorderTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(1, 1, 1, (sin(_Time.w * 5) * 0.5 + 0.5) * 0.2);
			}
			ENDCG
		}
	}
}
