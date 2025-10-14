Shader "UI/FastBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 4)) = 2.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // x = 1/width, y = 1/height
            float _BlurSize;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float4 color : COLOR;
                float4 worldPos : TEXCOORD1;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;
                o.worldPos = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // simple 9-tap separable-ish blur
                float2 px = _MainTex_TexelSize.xy * _BlurSize;

                fixed4 c = tex2D(_MainTex, i.uv) * 0.227027f;
                c += tex2D(_MainTex, i.uv + float2(px.x, 0)) * 0.1945946f;
                c += tex2D(_MainTex, i.uv - float2(px.x, 0)) * 0.1945946f;
                c += tex2D(_MainTex, i.uv + float2(0, px.y)) * 0.1216216f;
                c += tex2D(_MainTex, i.uv - float2(0, px.y)) * 0.1216216f;
                c += tex2D(_MainTex, i.uv + float2(px.x, px.y)) * 0.075f;
                c += tex2D(_MainTex, i.uv + float2(-px.x, px.y)) * 0.075f;
                c += tex2D(_MainTex, i.uv + float2(px.x, -px.y)) * 0.075f;
                c += tex2D(_MainTex, i.uv + float2(-px.x, -px.y)) * 0.075f;

                return c;
            }
            ENDHLSL
        }
    }
    FallBack "UI/Default"
}
