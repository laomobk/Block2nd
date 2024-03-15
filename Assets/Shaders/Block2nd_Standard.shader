Shader "B2nd/Block2nd_Standard"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		
		LOD 100
		
		// Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 light : COLOR;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 light : TEXCOORD2;
				float4 vertex : SV_POSITION;
				float lambert : TEXCOORD1;
				
				UNITY_FOG_COORDS(3)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.light = v.light;
				o.lambert = clamp(dot(v.normal, normalize(WorldSpaceLightDir(v.vertex))), 0.2, 1) + 0.35;
				
				UNITY_TRANSFER_FOG(o,o.vertex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, i.uv);
				fixed3 col = texColor.xyz * i.light.x * i.lambert;
				
				UNITY_APPLY_FOG(i.fogCoord, col);

				return fixed4(col, texColor.a);
			}
			ENDCG
		}
	}
}
