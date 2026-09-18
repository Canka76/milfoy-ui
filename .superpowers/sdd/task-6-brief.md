# Task 6 Brief: Diagnostic Shader — Unlit with Stripe Mode

## Files to Create
- `Editor/Resources/Shaders/UIQuadDiagnostic.shader`

## Exact Contents

```hlsl
Shader "Hidden/UIDepthInspector/QuadDiagnostic"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,0.7)
        _DiagnosticMode ("Diagnostic Mode", Float) = 0
        // 0 = solid, 1 = amber diagonal stripes
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
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

                // Stripe mode: diagonal amber stripes
                if (_DiagnosticMode > 0.5)
                {
                    float stripe = frac((i.uv.x + i.uv.y) * 8.0);
                    if (stripe > 0.5)
                        col.a *= 0.3;
                    else
                        col = fixed4(1.0, 0.75, 0.2, 0.85); // amber
                }

                return col;
            }
            ENDCG
        }
    }
}
```

## Instructions
1. Create directory `Editor/Resources/Shaders` if it does not exist.
2. Create `Editor/Resources/Shaders/UIQuadDiagnostic.shader` with the exact ShaderLab code specified.
3. Ensure proper formatting and LF line endings.
4. Stage with `git add Editor/Resources/Shaders/UIQuadDiagnostic.shader`.
5. Commit with message: `feat(viewport): add unlit diagnostic shader with stripe mode for ghost blockers`.
6. Write report to `.superpowers/sdd/task-6-report.md`.
