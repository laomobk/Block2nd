﻿Shader "B2nd/Block2nd_Repeat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Freq ("Frequence", float) = 10
        _BaseColor ("Base Color", Color) = (0, 0, 0, 1)
        
        [Header(UIProperties)]
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha


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
            float4 _BaseColor;
            float _Freq;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float aspect = _ScreenParams.y / _ScreenParams.x;

                float2 uv = i.uv;

                uv.x *= _Freq;
                uv.y *= aspect * _Freq;

                fixed4 col = tex2D(_MainTex, uv);
                return col * _BaseColor;
            }
            ENDCG
        }
    }
}
