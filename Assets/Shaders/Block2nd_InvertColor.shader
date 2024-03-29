Shader "B2nd/Block2nd_InvertColorSight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SightTex ("Sight Texture", 2D) = "white" {}
        _Scale ("Scale", float) = 0.1
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
            sampler2D _SightTex;
            float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float aspect = _ScreenParams.y / _ScreenParams.x;
                float height = _Scale;
                float width = _Scale * aspect;

                float beginX = 0.5 - width / 2;
                float beginY = 0.5 - height / 2;

                float2 uv = i.uv;
                float2 sightUV = float2(saturate((uv.x - beginX) / width), saturate((uv.y - beginY) / height));

                float4 col = tex2D(_MainTex, uv);
                float sight = tex2D(_SightTex, sightUV).x;
                
                return col * (1 - sight) + (1 - col) * sight;
            }
            ENDCG
        }
    }
}
