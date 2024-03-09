// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Post/GaussianBlur_Beauty" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _TextureSize ("_TextureSize",Float) = 256
        _BlurRadius ("_BlurRadius",Range(1,15) ) = 5
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass {
            NAME "GAUSSIAN_BLUR_PASS"

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"


            sampler2D _MainTex;
            int _BlurRadius;
            float _TextureSize;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };


            v2f vert( appdata_img v ) 
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            float GetGaussianDistribution( float x, float y, float rho ) {
                float g = 1.0f / sqrt( 2.0f * 3.141592654f * rho * rho );
                return g * exp( -(x * x + y * y) / (2 * rho * rho) );
            } 
            
            float4 GetGaussBlurColor( float2 uv )
            {
                float space = 1.0/_TextureSize; 
                float rho = (float)_BlurRadius * space / 3.0;

                float weightTotal = 0;
                for( int x = -_BlurRadius ; x <= _BlurRadius ; x++ )
                {
                    for( int y = -_BlurRadius ; y <= _BlurRadius ; y++ )
                    {
                        weightTotal += GetGaussianDistribution(x * space, y * space, rho );
                    }
                }

                float4 colorTmp = float4(0,0,0,0);
                for( int x = -_BlurRadius ; x <= _BlurRadius ; x++ )
                {
                    for( int y = -_BlurRadius ; y <= _BlurRadius ; y++ )
                    {
                        float weight = GetGaussianDistribution( x * space, y * space, rho )/weightTotal;

                        float4 color = tex2D(_MainTex,uv + float2(x * space,y * space));
                        color = color * weight;
                        colorTmp += color;
                    }
                }
                return colorTmp;
            }

            half4 frag(v2f i) : SV_Target 
            {
                return GetGaussBlurColor(i.uv);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}