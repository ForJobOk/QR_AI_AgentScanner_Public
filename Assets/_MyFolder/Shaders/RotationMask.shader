Shader "Custom/ProgressCircle"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ProgressCircleColor ("Progress Circle Color", Color) = (1, 1, 1, 1)
        _Speed ("Speed", Float) = 3
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _OverTex ("Overlay Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MaskTex;
            sampler2D _OverTex;
            float4 _MaskTex_ST;
            float4 _OverTex_ST;
            float _Speed;
            float4 _BaseColor;
            float4 _ProgressCircleColor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv_mask : TEXCOORD0;
                float2 uv_over : TEXCOORD1;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                
                float2 uv = v.uv * _MaskTex_ST.xy + _MaskTex_ST.zw;
                o.uv_mask = uv;

                float angle = _Time.y * _Speed;
                float s = sin(angle);
                float c = cos(angle);
                float2 center = float2(0.5, 0.5);
                
                float2 rotatedUV = mul(float2x2(c, -s, s, c), uv - center) + center;
                o.uv_over = rotatedUV * _OverTex_ST.xy + _OverTex_ST.zw;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mask = tex2D(_MaskTex, i.uv_mask);
                clip(mask.a - 0.5);

                fixed4 over = tex2D(_OverTex, i.uv_over);
                return lerp(_BaseColor,_ProgressCircleColor, over.r);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
