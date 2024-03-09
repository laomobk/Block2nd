Shader "Post/Post_BloomShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _TextureSize ("_TextureSize",Float) = 256       // For Gaussian
        _BlurRadius ("_BlurRadius",Range(1,15) ) = 5    // For Gaussian

        _PixelOffset ("Pixel Offset", float) = 0  // For Kawase

        _LuminanceThreshold ("Luminance Threshold", Range(0, 1)) = 0.5
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _LuminanceThreshold;

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
                fixed4 col = tex2D(_MainTex, i.uv);

                float threshold = clamp(luminance(col.rgb) - _LuminanceThreshold, 0, 1);

                return float4(threshold.xxx, 1) * col;
            }
            ENDCG
        }

        UsePass "Post/GaussianBlur_Beauty/GAUSSIAN_BLUR_PASS"

        // UsePass "Post/KawaseBlurShader/KAWASE_BLUR_PSSS"

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
            sampler2D _OriginalTex;

            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) + tex2D(_OriginalTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
