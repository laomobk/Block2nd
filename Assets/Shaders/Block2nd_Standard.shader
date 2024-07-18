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
				float4 light : COLOR;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 light : TEXCOORD2;
				float4 vertex : SV_POSITION;
				float lambert : TEXCOORD1;
				
				UNITY_FOG_COORDS(3)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _SkyLightColor;
			fixed4 _BlockLightColor;
			fixed4 _SkyHorizonColor;
			float _SkyLightLuminance;

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

				fixed4 skyLight = max(0.35, i.light.r) * _SkyLightColor;
				fixed4 blockLight = max(0.35, i.light.g) * _BlockLightColor;

				float blendFactor = min(i.light.r, _SkyLightLuminance);
				fixed4 blendedLight = (i.light.a * (blendFactor) * skyLight + max(1 - i.light.a, (1 - blendFactor) * blockLight));

				float lambert = max(0.6, i.light.r) * i.lambert;

				fixed3 col = texColor.xyz * lambert * blendedLight * (1 - i.light.b * 0.4f);
				
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, _SkyHorizonColor);

				// return fixed4((_SkyLightLuminance * (_SkyLightLuminance > 0.5f)).xxx, texColor.a);
				return fixed4(col, texColor.a);
			}
			ENDCG
		}
	}
}
