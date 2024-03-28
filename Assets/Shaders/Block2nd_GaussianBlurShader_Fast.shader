Shader "B2nd/GaussianBlurShader_Fast"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _BlurSize ("Blur Size", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            NAME "Gaussian_Vertical"

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
                float4 vertex : SV_POSITION;
                float2 uv[5] : TEXCOORD0;
            };

            sampler2D _MainTex;

            float4 _MainTex_ST;
            half4 _MainTex_TexelSize;

            float _BlurSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float2 uv = v.uv;

                float2 pxOffset = float2(0, _MainTex_TexelSize.y);

                o.uv[0] = uv;
                o.uv[1] = uv + pxOffset * 1 * _BlurSize;
                o.uv[2] = uv - pxOffset * 1 * _BlurSize;
                o.uv[3] = uv + pxOffset * 2 * _BlurSize;
                o.uv[4] = uv - pxOffset * 2 * _BlurSize;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float weight[3] = {0.4026, 0.2442, 0.0545};

                fixed3 col = tex2D(_MainTex, i.uv[0]) * weight[0];

                for (int j = 0; j < 2; j++) {
                    col += tex2D(_MainTex, i.uv[j * 2 + 1]).rgb * weight[j + 1];
                    col += tex2D(_MainTex, i.uv[j * 2 + 2]).rgb * weight[j + 1];
                }

                return fixed4(col.rgb, 1);
            }
            ENDCG
        }

        Pass
        {
            NAME "Gaussian_Horizontal"

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
                float4 vertex : SV_POSITION;
                float2 uv[5] : TEXCOORD0;
            };

            sampler2D _MainTex;

            float4 _MainTex_ST;
            half4 _MainTex_TexelSize;

            float _BlurSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float2 uv = v.uv;

                float2 pxOffset = float2(_MainTex_TexelSize.x, 0);

                o.uv[0] = uv;
                o.uv[1] = uv + pxOffset * 1 * _BlurSize;
                o.uv[2] = uv - pxOffset * 1 * _BlurSize;
                o.uv[3] = uv + pxOffset * 2 * _BlurSize;
                o.uv[4] = uv - pxOffset * 2 * _BlurSize;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float weight[3] = {0.4026, 0.2442, 0.0545};

                fixed3 col = tex2D(_MainTex, i.uv[0]) * weight[0];

                for (int j = 0; j < 2; j++) {
                    col += tex2D(_MainTex, i.uv[j * 2 + 1]).rgb * weight[j + 1];
                    col += tex2D(_MainTex, i.uv[j * 2 + 2]).rgb * weight[j + 1];
                }

                return fixed4(col.rgb, 1);
            }
            ENDCG
        }
    }
}
