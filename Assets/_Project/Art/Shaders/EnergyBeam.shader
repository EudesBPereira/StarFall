// Boss laser / lightning look: scrolling noise bands along the beam with a bright core.
Shader "Starfall/EnergyBeam"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 0.35, 0.55, 1)
        _Speed ("Scroll speed", Float) = 6
        _Bands ("Bands", Float) = 18
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off Lighting Off ZWrite Off Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _Speed;
            float _Bands;

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

            float hash(float n) { return frac(sin(n) * 43758.5453); }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                float core = 1.0 - abs(i.uv.x - 0.5) * 2.0;           // bright centre, soft edges
                float band = 0.5 + 0.5 * sin((i.uv.y + _Time.y * _Speed * 0.1) * _Bands);
                float jitter = hash(floor(i.uv.y * 40.0 + _Time.y * 30.0)) * 0.35;
                float wobble = 1.0 - abs(i.uv.x - 0.5 - (jitter - 0.17) * 0.3) * 2.0;
                float energy = saturate(core * core * 1.4 + wobble * band * 0.6);
                return fixed4(c.rgb * energy * 1.8, c.a * energy);
            }
            ENDCG
        }
    }
}
