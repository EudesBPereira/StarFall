// Energy shield ring: pulsing rim with a rotating highlight.
Shader "Starfall/ShieldSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.4, 0.8, 1, 0.6)
        _PulseSpeed ("Pulse speed", Float) = 4
        _Highlight ("Highlight strength", Range(0, 2)) = 0.8
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _PulseSpeed;
            float _Highlight;

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                float2 d = i.uv - 0.5;
                float angle = atan2(d.y, d.x);
                float sweep = 0.5 + 0.5 * sin(angle * 2.0 - _Time.y * _PulseSpeed);
                float pulse = 0.85 + 0.15 * sin(_Time.y * _PulseSpeed * 1.7);
                c.rgb *= pulse * (1.0 + _Highlight * sweep);
                return c * c.a;
            }
            ENDCG
        }
    }
}
