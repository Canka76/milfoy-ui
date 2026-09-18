Shader "Hidden/UIDepthInspector/QuadDiagnostic"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,0.7)
        _DiagnosticMode ("Diagnostic Mode", Float) = 0
        _Highlight ("Highlight Intensity", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float _DiagnosticMode;
            float _Highlight;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _Color;

                // 1. Stripe mode: diagonal amber stripes for ghost hitboxes
                if (_DiagnosticMode > 0.5 && _DiagnosticMode < 1.5)
                {
                    float stripe = frac((i.uv.x + i.uv.y) * 8.0);
                    if (stripe > 0.5)
                        col.a *= 0.3;
                    else
                        col = fixed4(1.0, 0.75, 0.2, 0.85); // amber
                }
                // 2. Inactive mode: fine cross-hatch stipple pattern for disabled objects
                else if (_DiagnosticMode > 1.5)
                {
                    float pattern = step(0.5, frac((i.uv.x + i.uv.y) * 16.0)) * step(0.5, frac((i.uv.x - i.uv.y) * 16.0));
                    col.rgb = fixed3(0.25, 0.25, 0.28);
                    col.a = pattern > 0.5 ? 0.45 : 0.12;
                }

                // 3. Highlight emission & neon rim glow for selected element
                if (_Highlight > 0.01)
                {
                    float edge = min(min(i.uv.x, 1.0 - i.uv.x), min(i.uv.y, 1.0 - i.uv.y));
                    float rim = smoothstep(0.04, 0.0, edge);
                    fixed3 neonCyan = fixed3(0.0, 0.92, 1.0);
                    col.rgb = lerp(col.rgb, neonCyan, rim * 0.85 * _Highlight);
                    col.a = saturate(col.a + rim * 0.35 * _Highlight);
                }

                return col;
            }
            ENDCG
        }
    }
}
