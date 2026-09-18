# UI Depth Inspector — 3D Visual Enhancements & Card Thickness Spec

**Date:** 2026-09-18
**Status:** Draft
**Component:** 3D Viewport Rendering & Visual Ergonomics
**Package:** `com.openupm.ui-depth-inspector`

---

## 1. Purpose & Motivation

In current builds, UI elements in the 3D viewport are rendered as flat 2D quads ($Z\text{-depth} = 0$). While functional from isometric angles, flat cards have critical visual weaknesses:
1. **Side View Invisibility**: When rotated to the 90° Side Elevation view preset, 2D quads have zero cross-sectional thickness and become completely invisible lines.
2. **Lack of Tactile Layer Separation**: Without depth on the card edges, overlapping layers blend together visually, making it harder to discern boundaries between adjacent cards.
3. **Thin Edge Raycast Picking**: Picking cards from glancing angles is difficult because flat colliders have no side profile.

This specification introduces **3D Extruded Slabs with Shaded Side Walls**, **High-Contrast Hitbox Outlines**, and **Dynamic Thickness Controls**.

---

## 2. Design Decisions

| Decision | Choice | Rationale |
|---|---|---|
| Card Geometry | 3D Extruded Slabs (Box primitive with 6 faces) | Provides physical thickness, dark-shaded side walls for depth contrast, and full side-profile visibility at 90° side elevation. |
| Side Wall Shading | Shaded side wall tint ($\approx 60\%$ luminance) | Differentiates front faces from side walls, creating natural ambient depth without heavy lighting overhead. |
| Hitbox Visualization | High-Contrast Glowing Edge Borders | Draws glowing perimeter rings on active raycast blockers (`raycastTarget == true`) and amber rings on ghost blockers. |
| Thickness Tuning | Toolbar Slider ($0.02$ to $0.5$ units) | Allows designers to tune card thickness dynamically to match their Canvas scale. |

---

## 3. Geometry & Mesh Architecture

### 3.1 3D Slab Generation (`CreateSlab`)
In `UIPreview3DViewport.cs`:
- Replace 2D `PrimitiveType.Quad` with 3D `PrimitiveType.Cube` (or procedural 24-vertex box mesh).
- **Front Face ($+Z$)**: Renders the diagnostic material (Red for blocker, Cyan for passive, Gray for inactive, Amber diagonal stripes for ghost blocker).
- **Side Faces (Top, Bottom, Left, Right)**: Shaded darker to simulate studio side ambient occlusion.
- **Colliders**: `BoxCollider` seamlessly encloses the 3D card volume, allowing accurate raycast picking on front and side surfaces.

### 3.2 Transform & Positioning Math
In `PositionQuad`:
```csharp
float normX = (entry.WorldRect.center.x - _normalizationRect.x) / _normalizationRect.width * 8f - 4f;
float normY = (entry.WorldRect.center.y - _normalizationRect.y) / _normalizationRect.height * 8f - 4f;
float z = totalCount > 0 ? index * (_explosionFactor / Mathf.Max(totalCount, 1)) : 0f;

// Set position centered in Z-depth
t.localPosition = new Vector3(normX, normY, z);

// Scale X and Y to match element dimensions, scale Z to slabThickness
float scaleX = Mathf.Max(entry.WorldRect.width / _normalizationRect.width * 8f, 0.05f);
float scaleY = Mathf.Max(entry.WorldRect.height / _normalizationRect.height * 8f, 0.05f);
float scaleZ = Mathf.Max(_slabThickness, 0.02f);

t.localScale = new Vector3(scaleX, scaleY, scaleZ);
```

---

## 4. Visual Enhancements & Edge Rendering

### 4.1 High-Contrast Hitbox Outlines
In `UIPreview3DViewport.OnGUI`:
- For elements with `DiagnosticFlags.RaycastBlocker`: draw high-contrast coral/red front border lines.
- For elements with `DiagnosticFlags.GhostBlocker`: draw glowing amber/gold front border lines.
- For elements with `DiagnosticFlags.HasMask` or `DiagnosticFlags.HasRectMask2D`: draw green dashed wireframe bounds.

### 4.2 Selection Highlight Cage
- Selected element receives a bright yellow bounding cage (`Handles.DrawWireCube`) surrounding the extruded 3D volume.

---

## 5. Toolbar Controls

- Add a **Thickness** slider (`0.02` to `0.5`, default `0.12`) on the top toolbar row alongside `Z-Explosion`.
- Adjusting the slider updates all preview slab transforms in real-time.

---

## 6. Verification & Self-Review

1. **Placeholder Scan**: Verified zero TBDs or un-specced variables.
2. **Consistency Check**: Math formulas, coordinate normalization, and material categories align with core design docs.
3. **Scope Check**: Focused strictly on 3D viewport rendering enhancements and controls.
