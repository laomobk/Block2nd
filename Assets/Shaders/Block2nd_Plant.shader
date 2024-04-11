﻿Shader "B2nd/Block2nd_Plant"
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
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
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
				o.lambert = clamp(dot(v.normal, normalize(WorldSpaceLightDir(v.vertex))), 0.2, 1) + 0.3;
				
				UNITY_TRANSFER_FOG(o,o.vertex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, i.uv);

				if (texColor.a == 0)
					discard;

				fixed3 col = texColor.xyz;
				
				UNITY_APPLY_FOG(i.fogCoord, col);

				return fixed4(col, texColor.a);
			}
			ENDCG
		}
		
		Pass
        {
            Name "Klee's Shadow Caster"
            Tags {
                "LightMode"="ShadowCaster"
            }

            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc" 

            struct vin {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;

            v2f vert(vin v) { 
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.pos = UnityApplyLinearShadowBias(o.pos);
                
                o.uv = v.uv;

                return o;
            }

            float4 frag(v2f i) : SV_TARGET {
                if (tex2D(_MainTex, i.uv).a == 0)
                {
                    discard;
                }
                return 0;
            }

            ENDCG
        }
	}
}