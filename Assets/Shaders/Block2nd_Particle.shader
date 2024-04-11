Shader "B2nd/Block2nd_Particle"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		_Texcoord ("Texcoord", Vector) = (0, 0, 0, 0) 
	}
	SubShader
	{
		Tags { "Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent" }

		Blend SrcAlpha OneMinusSrcAlpha

		LOD 100

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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float2 _Texcoord;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, _Texcoord + i.uv / 32);
				if (col.a == 0)
					discard;
				return col;
			}
			ENDCG
		}
	}
}
