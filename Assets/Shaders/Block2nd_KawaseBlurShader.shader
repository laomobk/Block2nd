Shader "B2nd/KawaseBlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _PixelOffset ("Pixel Offset", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            NAME "KAWASE_BLUR_PSSS"

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
            float4 _MainTex_TexelSize;

            float _PixelOffset;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 texelSize = _MainTex_TexelSize.xy;

                half4 col = 0;
	        	col += tex2D(_MainTex, i.uv + float2(_PixelOffset + 0.5, _PixelOffset + 0.5) * texelSize); 
	        	col += tex2D(_MainTex, i.uv + float2(-_PixelOffset - 0.5, _PixelOffset + 0.5) * texelSize); 
	        	col += tex2D(_MainTex, i.uv + float2(-_PixelOffset - 0.5, -_PixelOffset - 0.5) * texelSize); 
	        	col += tex2D(_MainTex, i.uv + float2(_PixelOffset + 0.5, -_PixelOffset -0.5) * texelSize); 

	        	return col * 0.25;

                return col;
            }
            ENDCG
        }
    }
}
