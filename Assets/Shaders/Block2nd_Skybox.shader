Shader "B2nd/Block2nd_Skybox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _HorizonColor ("Horizon Color", Color) = (1, 1, 1)
        _TopColor ("Top Color", Color) = (1, 1, 1)
        _BlendRatio ("Blend Ratio", Range(-1, 1)) = 0.5
        _StepRatio("Step Ratio", Range(1, 10)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float3 pos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            float4 _MainTex_ST;

            float4 _HorizonColor, _TopColor;

            float _BlendRatio, _StepRatio;

            float sigmoid(float fact_x, float ofs_x, float x)
            {
                return 1 / (1 + exp(-6 * fact_x * (2 * x - ofs_x - 1)));
            }

            float4 skycolor(float3 v)
            {
                return lerp(_HorizonColor, _TopColor, saturate(sigmoid(_StepRatio, _BlendRatio, abs(v.y))));
            }

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.pos = v.vertex;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 v = normalize(i.pos.xyz);

                float4 col = skycolor(v);

                return col;
            }
            ENDCG
        }
    }
}
