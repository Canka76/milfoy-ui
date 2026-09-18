# UI Depth Inspector — 3D Visual Enhancements Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement 3D physical card thickness (extruded slabs), shaded side walls, high-contrast hitbox borders, and dynamic thickness slider controls in the UI Depth Inspector window — step by step with isolated verification.

**Architecture:** Replace flat 2D `Quad` primitives with 3D `Cube` slabs in `UIPreview3DViewport.cs`. Position and scale slabs in XYZ with uniform Z-thickness. Add a Thickness slider to `UIToolbarPanel.cs` and `UIDepthInspector.uxml`. Add high-contrast hitbox edge rendering to `OnGUI`.

**Tech Stack:** Unity 6 (6000.0+), C#, ShaderLab, UI Toolkit.

## Global Constraints
- Minimal, clean step-by-step changes.
- Zero GC allocations in hot rendering and collection loops.
- All commits follow Conventional Commits format (`feat:`, `fix:`, `refactor:`, `test:`).
- Sync all modifications to `Packages/com.openupm.ui-depth-inspector/`.

---

## Tasks

### Task 1: 3D Extruded Slab Geometry & Volume Scaling
- Replace 2D Quad with 3D Cube primitive in `UIPreview3DViewport.cs`.
- Update `PositionQuad` to scale Z by `_slabThickness` (default `0.15f`).
- Configure `BoxCollider` on all slabs for full 3D picking across front and side walls.

### Task 2: Dynamic Thickness Slider on Toolbar
- Add a `Thickness` slider (`0.02` to `0.5`, default `0.15`) in `UIDepthInspector.uxml`.
- Bind in `UIToolbarPanel.cs` and wire `OnThicknessChanged` to `_viewport.SetSlabThickness(factor)`.

### Task 3: High-Contrast Hitbox Outlines & Selection Highlights
- Draw glowing coral outlines for active blockers and amber gold outlines for ghost blockers in `OnGUI`.
- Highlight selected slab with yellow 3D bounding cage.

---

## File Map
| File | Responsibility |
|---|---|
| `Assets/UIDepthInspector/Editor/Viewport/UIPreview3DViewport.cs` | 3D Slab creation, Z-scaling, thickness property, hitbox outlines |
| `Assets/UIDepthInspector/Editor/Resources/UIDepthInspector.uxml` | Toolbar Thickness slider UI |
| `Assets/UIDepthInspector/Editor/Panels/UIToolbarPanel.cs` | Thickness slider binding and callback |
| `Assets/UIDepthInspector/Editor/UIDepthInspectorWindow.cs` | Wiring thickness callback to viewport |
