Shader "B2nd/Block2nd_Clouds"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100
		
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		    Cull off
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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
				float4 vertex : SV_POSITION;
				
				UNITY_FOG_COORDS(4)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float3 _PlayerPos;
			
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
				i.uv.x += _Time.x / 100;
				i.uv.x -= _PlayerPos.x / 3000;
				i.uv.y -= _PlayerPos.z / 3000;

				fixed4 col = tex2D(_MainTex, i.uv);

				if (col.a < 0.1)
					discard;
				
				UNITY_APPLY_FOG(i.fogCoord, col);

				col.a = 0.75f;
				
				return col;
			}
			ENDCG
		}
	}
}
