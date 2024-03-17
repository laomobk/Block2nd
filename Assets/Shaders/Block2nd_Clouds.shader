Shader "B2nd/Block2nd_Clouds"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		_Strength ("Strength", range(1, 100)) = 10
	}
	SubShader
	{
		Tags { "Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100
		
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		    Cull off
		    // Offset -2, -2
		
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
			float _Strength;
			
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
				float u = i.uv.x;
				float v = i.uv.y;

				u = (1 / _Strength)* round(u / (1 / _Strength));
				v = (1 / _Strength)* round(v / (1 / _Strength));

				fixed4 col = tex2D(_MainTex, float2(u, v));

				float val = step(col.x, 0.6);

				if (val < 0.1)
					discard;
				
				return fixed4(val, val, val, 1);
			}
			ENDCG
		}
	}
}
