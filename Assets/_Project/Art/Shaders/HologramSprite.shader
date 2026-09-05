// Placeholder hologram look for the Hangar preview: scanlines, cyan tint, flicker.
Shader "Starfall/HologramSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.4, 0.9, 1, 1)
        _Flicker ("Flicker", Range(0, 1)) = 1
        _ScanDensity ("Scanline density", Float) = 120
        _ScanStrength ("Scanline strength", Range(0, 1)) = 0.35
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _Flicker;
            float _ScanDensity;
            float _ScanStrength;

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
                float scan = 1.0 - _ScanStrength * (0.5 + 0.5 * sin((i.uv.y + _Time.y * 0.15) * _ScanDensity));
                float edge = 0.6 + 0.4 * sin(_Time.y * 3.0 + i.uv.y * 20.0);
                c.rgb *= scan * _Flicker * edge;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
