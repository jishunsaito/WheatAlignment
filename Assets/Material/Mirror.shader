Shader "Custom/Mirror"
{
Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _FlipX ("Horizontal Flip", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            HLSLPROGRAM

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
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _FlipX;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // 水平方向に反転
                if (_FlipX > 0.5)
                {
                    uv.x = 1.0 - uv.x;
                }

                return tex2D(_MainTex, uv);
            }

            ENDHLSL
        }
    }

    Fallback Off
}
