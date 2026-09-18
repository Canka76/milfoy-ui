# Refined 3D Selection Glow & HUD Reticle Corner Brackets Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refine the 3D viewport selection visuals by replacing harsh surface whiteout with a fine beveled perimeter neon rim in the shader and drawing precision HUD 3-axis corner reticle brackets with gentle pulsing in the 3D viewport.

**Architecture:** `UIQuadDiagnostic.shader` blends neon cyan only on a narrow boundary rim (`smoothstep(0.04, 0.0, edge)`). `UIPreview3DViewport.cs` draws 8 vertex corner L-brackets and a subtle bounding wireframe with balanced $0.65\times$ unselected layer dimming.

**Tech Stack:** Unity 6 C#, PreviewRenderUtility / Handles, ShaderLab / HLSL.

## Global Constraints
- Target Unity 6000.7+
- Dual sync across `Assets/UIDepthInspector/` and `Packages/com.openupm.ui-depth-inspector/`
- Zero per-frame allocations during 3D preview render loops

---

### Task 1: Shader Beveled Perimeter Rim Glow

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Resources/Shaders/UIQuadDiagnostic.shader`
- Modify: `Packages/com.openupm.ui-depth-inspector/Editor/Resources/Shaders/UIQuadDiagnostic.shader`

- [ ] **Step 1: Update fragment shader in `UIQuadDiagnostic.shader`**
Update `_Highlight` condition to calculate `edge = min(min(i.uv.x, 1.0 - i.uv.x), min(i.uv.y, 1.0 - i.uv.y))` and lerp neon cyan only over `rim = smoothstep(0.04, 0.0, edge)`, preserving core card color without white bleed.

- [ ] **Step 2: Mirror changes to `Packages/com.openupm.ui-depth-inspector/`**

---

### Task 2: HUD Reticle Corner Brackets & Balanced Dimming

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Viewport/UIPreview3DViewport.cs`
- Modify: `Packages/com.openupm.ui-depth-inspector/Editor/Viewport/UIPreview3DViewport.cs`

- [ ] **Step 1: Implement `DrawCornerReticleBrackets` in `UIPreview3DViewport.cs`**
Replace three heavy wireframe cubes with a single subtle bounding box (`Color(0f, 0.85f, 1f, 0.30f)`) and 8 vertex 3-axis corner L-brackets (`Color(0f, 0.95f, 1f, 0.95f)`).

- [ ] **Step 2: Balance unselected card dimming in `UpdateQuadColors`**
Adjust unselected alpha multiplier to `0.65f` (from harsh `0.40f`) for readable surrounding context.

- [ ] **Step 3: Mirror changes to `Packages/com.openupm.ui-depth-inspector/`**

---

### Task 3: Verification & Live Visual Check

- [ ] **Step 1: Check Unity Editor live log stream**
Verify clean compilation with zero shader errors or script exceptions.
