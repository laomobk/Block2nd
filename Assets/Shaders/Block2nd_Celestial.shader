Shader "B2nd/Block2nd_Celestial"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Background" }
		LOD 100
		
		Blend One OneMinusSrcAlpha 
		ZTest Less

		Pass
		{
			Cull off
			ZTest Less
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma multi_compile_fog
			
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
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
            float luminance(float3 color) {
                return color.r * 0.2125 + color.g * 0.7154 + color.b * 0.0721;
            }
			
			v2f vert (appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, i.uv);

				texColor.a = luminance(texColor.xyz);

				return texColor;
			}
			ENDCG
		}
	}
}
