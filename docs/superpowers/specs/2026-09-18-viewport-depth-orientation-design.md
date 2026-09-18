# 3D Viewport Depth Direction & Canvas Orientation Design

## Problem
In `UIPreview3DViewport.cs`, the camera is positioned at $Z = -\_zoomDistance$ (negative Z) facing forward toward the origin ($+Z$). However, quads are positioned along positive Z:
```csharp
float z = totalCount > 0 ? index * (_explosionFactor / Mathf.Max(totalCount, 1)) : 0f;
```
This causes higher draw order (foreground) elements to be pushed deeper along $+Z$ (further away from the camera), while background elements remain closer to the camera at $Z = 0$. When looking from the Front view preset, sorting order appears backwards.

## Solution

### 1. Invert Layer Stacking Along Z
Change the Z-offset in `PositionQuad` so higher draw order layers stack toward the camera (negative Z):
```csharp
float z = totalCount > 0 ? -index * (_explosionFactor / Mathf.Max(totalCount, 1)) : 0f;
```
- Draw Order 0 (Background): $Z = 0$.
- Draw Order N (Foreground / Top-most): $Z = -N \times \text{step}$ (closest to camera).

### 2. View Angles & Presets
- **Front View**: Camera at $(0, 0, -10)$, looking at $(0, 0, 0)$. Foreground elements are directly in front, matching 2D screen sorting.
- **Isometric View**: Camera at $(30^\circ, 45^\circ)$, showing clearly that layer 0 is the base and higher layers pop out forward toward the viewer.
- **Side View**: Layer 0 on the right/back, higher layers extending toward the left/front.

### 3. Raycasting & Picking
- 3D picking via raycasting through `_colliders` automatically functions correctly with negative Z coordinates because ray intersection respects world space transform positions.

### 4. Verification
- Verify via Unity Editor tests (`Assets/UIDepthInspector/Tests/Editor/`) ensuring viewport quad placement ordering invariants hold.
