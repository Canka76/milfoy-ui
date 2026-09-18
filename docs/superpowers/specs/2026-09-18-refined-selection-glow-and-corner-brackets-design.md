# Spec: Refined 3D Selection Glow & HUD Reticle Corner Brackets

## Overview
This specification refines the 3D viewport selection aesthetics for the Milfoy UI Depth Inspector, replacing harsh white/cyan surface bleeding and thick wireframe cubes with a beveled perimeter rim glow and high-precision HUD reticle corner target brackets.

---

## 1. Problem Statement & Goals

### Current Issues Observed
- The previous shader glow logic saturated the entire card with cyan/white, washing out the underlying red/green/custom colors and making text/icons unreadable.
- Drawing multiple full wireframe cubes created clutter rather than an elegant, readable target.

### Goals
- **Color Preservation**: The center of the selected card maintains 100% of its authentic diagnostic or custom color with zero whiteout bleed.
- **Crisp Bevel Rim**: A fine 2–3 pixel beveled neon rim along the edge of the card providing clear definition.
- **HUD Reticle Corner Brackets**: Sleek 3-axis corner L-brackets at the 8 vertices of the selected bounding box with gentle breathing animation.
- **Zero GC Allocations**: All calculations use local float math and cached matrix transforms.

---

## 2. Technical Specifications

### 2.1 `UIQuadDiagnostic.shader`
- Calculate UV edge distance: `edge = min(min(i.uv.x, 1.0 - i.uv.x), min(i.uv.y, 1.0 - i.uv.y))`
- Smoothstep fine rim: `rim = smoothstep(0.04, 0.0, edge)`
- Fragment color:
  ```hlsl
  if (_Highlight > 0.01)
  {
      float edge = min(min(i.uv.x, 1.0 - i.uv.x), min(i.uv.y, 1.0 - i.uv.y));
      float rim = smoothstep(0.04, 0.0, edge);
      fixed3 neonCyan = fixed3(0.0, 0.92, 1.0);
      col.rgb = lerp(col.rgb, neonCyan, rim * 0.85 * _Highlight);
      col.a = saturate(col.a + rim * 0.35 * _Highlight);
  }
  ```

### 2.2 `UIPreview3DViewport.cs` Reticle Brackets
- **Single Subtle Outer Frame**: Thin bounding wireframe with `Color(0f, 0.85f, 1f, 0.30f)`.
- **8 Vertex Corner L-Brackets**: For each vertex of the bounding box, draw 3 short orthogonal line segments of length `bracketLen = min(size.x, size.y) * 0.18f` in vibrant Neon Cyan (`Color(0f, 0.95f, 1f, 0.95f)`).
- **Pulsing Dynamics**:
  `float pulse = 0.5f + 0.5f * Mathf.Sin((float)EditorApplication.timeSinceStartup * 3.0f);`
- **Balanced Unselected Dimming**: Unselected card alpha multiplier set to `0.65f`.

---

## 3. Testing & Verification
- Verify that selected cards retain their exact hue (e.g. vivid red, blue, green) while displaying the fine cyan beveled rim and corner brackets.
- Verify that unselected cards remain readable without excessive dimming.
- Confirm zero per-frame allocations during interactive orbiting and pulsing.
