# UI Layer & 3D Depth Inspector — Design Spec

**Date:** 2026-09-18
**Status:** Draft
**Unity Target:** 6000.0+ (Unity 6)
**Distribution:** UPM package via Git URL
**Package ID:** `com.openupm.ui-depth-inspector`

---

## 1. Purpose

A dockable, resizable Unity `EditorWindow` inspired by browser 3D DOM visualizers (Chrome/Edge 3D View). It renders flat uGUI Canvases as an interactive 3D exploded viewport alongside a synchronized stack list, enabling UI designers and technical artists to debug:

1. Render stack and draw ordering (back-to-front layer index).
2. Overlapping UI bounds and visual occlusion.
3. Hidden/accidental raycast blockers (transparent or alpha-zero graphics).
4. Mask boundaries and layer isolation.

Works in both Edit Mode and Play Mode, real-time.

---

## 2. Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Viewport rendering | `PreviewRenderUtility` | Real 3D scene with free raycasting, depth buffer, and orbit camera. Stable API since Unity 5. Less code than alternatives. |
| Editor chrome | UI Toolkit + `IMGUIContainer` for viewport | Modern data binding, USS styling, `TwoPaneSplitView` built-in. `IMGUIContainer` hosts the `RenderTexture` blit. |
| Quad fidelity (v1) | Solid diagnostic color planes | Delivers full debugging value. Textured quads (render actual UI content) deferred to Phase 2. |
| Minimum Unity version | 6000.0 (Unity 6) | No backport complexity. Full UI Toolkit and `PreviewRenderUtility` support. |
| Configuration storage | `EditorPrefs` | No ScriptableObject or settings asset for v1. |
| Dependencies | `com.unity.ugui` only | Zero external dependencies. |

---

## 3. Package Structure

```
com.openupm.ui-depth-inspector/
├── package.json
├── README.md
├── LICENSE.md                            # MIT
├── CHANGELOG.md
├── Editor/
│   ├── UIDepthInspector.Editor.asmdef
│   ├── UIDepthInspectorWindow.cs         # EditorWindow host, UXML/USS root
│   ├── Core/
│   │   ├── UIRenderTreeCollector.cs      # Traversal engine → flattened draw-order list
│   │   ├── UIElementEntry.cs             # Data struct per collected element
│   │   └── UIRenderTreeCache.cs          # Dirty-check + caching layer
│   ├── Viewport/
│   │   ├── UIPreview3DViewport.cs        # PreviewRenderUtility wrapper, camera controls
│   │   ├── UIExplosionMeshBuilder.cs     # Builds quads in preview scene from entries
│   │   └── UIViewportPicker.cs           # Raycast picking in preview scene
│   ├── Panels/
│   │   ├── UIStackListPanel.cs           # Right-pane scrollable list (UI Toolkit)
│   │   └── UIToolbarPanel.cs             # Toolbar: presets, slider, filters, canvas picker
│   ├── Diagnostics/
│   │   ├── UIDiagnosticAnalyzer.cs       # Ghost blocker detection, mask bounds, CanvasGroup
│   │   └── UIDiagnosticBadges.cs         # Badge/icon rendering for warnings
│   ├── Resources/
│   │   ├── UIDepthInspector.uss
│   │   ├── UIDepthInspector.uxml
│   │   └── Shaders/
│   │       └── UIQuadDiagnostic.shader   # Unlit, vertex-colored, transparent, double-sided
│   └── Icons/                            # Eye, raycast, solo, warning icons
```

No `Runtime/` assembly — pure editor tool. Assembly definition references `UnityEditor`, `UnityEngine`, `UnityEngine.UI`.

---

## 4. Render Tree Collection Engine

### Algorithm — `UIRenderTreeCollector.Collect()`

1. **Find root Canvases** via `FindObjectsByType<Canvas>(FindObjectsSortMode.None)`, filter to `canvas.isRootCanvas == true`.

2. **Sort root Canvases** by `(SortingLayer.GetLayerValueFromID(sortingLayerID), sortingOrder)`.

3. **Pre-order depth-first traversal** within each root Canvas, following `Transform.GetSiblingIndex()` order.

4. **At each node:**
   - `Canvas` with `overrideSorting == true` → defer subtree; re-insert at correct global position by its own sort key.
   - `Graphic` component → emit `UIElementEntry` with current global draw index.
   - `CanvasGroup` → record cumulative `alpha` (multiplicative) and `blocksRaycasts` (AND) for descendants.

5. **Merge deferred override-sorting subtrees** into the global list by sort key. Within each subtree, same pre-order DFS applies.

6. **Assign global stack indices** `#00` through `#N` in final draw order.

### `UIElementEntry` (struct)

```csharp
struct UIElementEntry
{
    int          globalDrawIndex;    // #00 = backmost
    string       name;               // gameObject.name
    bool         isActive;           // gameObject.activeInHierarchy
    bool         raycastTarget;      // graphic.raycastTarget
    float        effectiveAlpha;     // graphic.color.a * cumulative CanvasGroup.alpha
    bool         blocksRaycasts;     // cumulative CanvasGroup.blocksRaycasts
    Rect         worldRect;          // RectTransform world-space bounds
    RenderMode   canvasRenderMode;   // Screen Space Overlay/Camera, World Space
    int          rootCanvasId;       // instance ID for Canvas-level filtering
    string       rootCanvasName;     // display name for Canvas filter dropdown
    DiagnosticFlags diagnosticFlags; // GhostBlocker, HasMask, HasRectMask2D, Inactive, etc.
    Transform    transform;          // reference back to scene object
}
```

### Cache Strategy — `UIRenderTreeCache`

- **Invalidation triggers:** `EditorApplication.hierarchyChanged`, `Undo.undoRedoPerformed`, manual refresh button.
- **Not per-frame.** Explosion slider, filter changes, and camera moves read from cache; they do not trigger recollection.
- Stores `UIElementEntry[]` and a generation counter.

### World Space Canvas Handling

- World Space canvases participate in the same sort-key ordering.
- `worldRect` from `RectTransformUtility.CalculateRelativeRectTransformBounds` in world coordinates.
- Preview scene quads are scaled to normalized preview coordinates (largest canvas dimensions set the reference frame).

---

## 5. 3D Viewport & Camera

### `UIPreview3DViewport` — PreviewRenderUtility Wrapper

**Lifecycle:** Created on `OnEnable`, destroyed on `OnDisable`.

**Preview Scene Objects:**
- One `GameObject` per `UIElementEntry`, each with `MeshFilter` + `MeshRenderer` holding a single-quad mesh.
- 4 shared materials (one per diagnostic category: red/raycast, cyan/passive, gray/inactive, amber-striped/ghost).
- Objects created from cache, repositioned when explosion slider moves.

### Mesh Building — `UIExplosionMeshBuilder`

- **Quad XY position:** From element's `worldRect` center, normalized to preview coordinates.
- **Quad Z position:** `globalDrawIndex * explosionFactor` where `explosionFactor = sliderValue / totalElementCount`, giving uniform spacing.
- **Quad size:** Matches element's RectTransform width/height in normalized preview units.
- **Material assignment:** Per the diagnostic color scheme (see Section 7).

### Camera Controls

Processed in `IMGUIContainer.onGUIHandler` from `Event.current`:

| Input | Action |
|-------|--------|
| Right-click drag | Orbit camera around pivot (center of quad stack) via `Quaternion.AngleAxis` |
| Middle-click drag (or Alt+Left-click) | Pan: translate pivot in camera-local XY plane |
| Scroll wheel | Zoom: move camera along forward axis (perspective) or adjust orthographic size |

### View Presets

| Preset | Camera Position | Projection |
|--------|----------------|------------|
| Front (True 2D) | `(0, 0, -distance)`, forward `+Z` | Orthographic |
| Isometric 45° | Rotated 30° X, 45° Y | Perspective |
| Side Elevation | `(-distance, 0, 0)`, forward `+X` | Perspective |

### Picking — `UIViewportPicker`

- On left-click in viewport, raycast from mouse screen position through `PreviewRenderUtility.camera` into the preview scene.
- Hit detection via `Physics.Raycast` against `MeshCollider` components on preview quads (auto-added during mesh build).
- Hit → map preview GameObject back to `UIElementEntry` → set `Selection.activeGameObject = entry.transform.gameObject`.

### Selection Highlight

- Selected element's quad receives a wireframe bounding cage drawn via `GL.Begin(GL.LINES)` in an overlay pass — bright yellow (#FFFF00), 2px equivalent.

---

## 6. Dual-Pane Editor Window

### Layout

```
┌─────────────────────────────────────────────────────────┐
│ Toolbar                                                  │
│ [Front] [Iso] [Side] │ Z-Explosion: ═══●═══ 12.5        │
│ [Search ___________]  [Raycast Only] [Warnings] [Active] │
├────────────────────────────────┬─────────────────────────┤
│                                │  Stack List             │
│   3D Viewport                  │  #00 ● Background       │
│   (IMGUIContainer)             │  #01 ● Panel_Main       │
│                                │  #02 ⚠ InvisibleBlock   │
│                                │  #03 ● Btn_Start        │
│                                │  #04 ● Txt_Label        │
│                                │                         │
│                                │  Canvas: [All ▾]        │
│                                │  [Copy Stack]           │
├────────────────────────────────┴─────────────────────────┤
│ Status: 42 elements │ 3 warnings │ Canvas: MainHUD       │
└─────────────────────────────────────────────────────────┘
```

- **Split:** `TwoPaneSplitView` — left = `IMGUIContainer` (viewport), right = `ScrollView` (stack list). User-resizable.
- **Menu path:** `Window/UI/UI Depth Inspector`

### Stack List — `UIStackListPanel`

Each row: `[#index badge] [color dot] [name] [⚠ if ghost] [👁 active] [◎ raycast] [🔘 solo]`

- Clicking a row sets `Selection.activeGameObject` → viewport highlights, native Hierarchy syncs.
- Selected row gets highlight background; auto-scrolls into view.
- `ListView` with virtualization for 500+ element performance.

### Bi-Directional Selection Sync

Single source of truth: **Unity's `Selection`**. Both panes subscribe to `Selection.selectionChanged`:

- Selection change → find matching `UIElementEntry` → highlight in viewport + scroll list.
- Viewport pick → set `Selection.activeGameObject` → callback fires → list syncs.
- External Hierarchy click → same callback → both panes sync.

### Toolbar — `UIToolbarPanel`

- View preset buttons → `viewport.SetViewPreset(preset)`.
- Explosion slider: `Slider`, range `0.0`–`50.0`, fires `viewport.SetExplosionFactor(value)`.
- Filter chips: `Toggle` buttons. Applied to both list and viewport (filtered-out elements hidden in preview scene).
- Search: `TextField`, live filter by `name.Contains(query, OrdinalIgnoreCase)`.
- Canvas filter: `DropdownField`, populated from distinct root Canvas names. "All" default.

### Repaint Strategy

- Viewport repaints via `EditorApplication.update` only when dirty (camera moved, slider changed, selection changed).
- List repaints on cache invalidation or filter change.
- Not every frame.

---

## 7. Diagnostic Visualizers

### Diagnostic Flags — `UIDiagnosticAnalyzer`

Computed once per cache rebuild:

| Flag | Condition | Viewport Color | List Treatment |
|------|-----------|---------------|----------------|
| Raycast Blocker | `raycastTarget && effectiveAlpha > 0 && isActive` | Red/Coral `#E06060` alpha 0.7 | Red dot |
| Ghost Blocker | `raycastTarget && effectiveAlpha == 0 && isActive` OR `raycastTarget && graphic.sprite == null && graphic is Image && isActive` | Amber striped quad | "⚠ Invisible Hitbox" badge |
| Passive Visual | `!raycastTarget && isActive` | Cyan `#60A0E0` alpha 0.5 | Blue dot |
| Inactive | `!gameObject.activeInHierarchy` | Gray `#808080` alpha 0.3 | Dimmed row |
| Group Blocked | Cumulative `CanvasGroup.blocksRaycasts == false` | Override to Cyan (even if `raycastTarget == true`) | Blue dot + note |
| Group Transparent | Cumulative `CanvasGroup.alpha == 0` + `raycastTarget` + `blocksRaycasts` | Amber striped (ghost blocker) | "⚠ Invisible Hitbox" badge |
| Has Mask | `GetComponent<Mask>() != null` | Green dashed wireframe rect at quad position | Mask icon |
| Has RectMask2D | `GetComponent<RectMask2D>() != null` | Green solid wireframe rect at quad position | Mask icon |

### CanvasGroup Stacking

Walk up from each element through ancestors. For each `CanvasGroup`:
- `effectiveAlpha *= canvasGroup.alpha`
- `blocksRaycasts &&= canvasGroup.blocksRaycasts`

Matches uGUI's runtime behavior.

### Amber Stripe Shader — `UIQuadDiagnostic.shader`

- Unlit, double-sided, transparent.
- `_DiagnosticMode` property: `0` = flat vertex color, `1` = diagonal stripes via `frac(uv.x + uv.y)` in fragment shader. No texture needed.

### Mask Bounds in Viewport

For `Mask` and `RectMask2D` components, draw wireframe rectangle at the element's quad Z-position in preview scene using `GL.Begin(GL.LINES)`. Color: green `#40E040`.

---

## 8. Quick Controls & Undo

### Per-Row Controls

| Control | Action | Undo Call |
|---------|--------|-----------|
| Eye toggle | `Undo.RecordObject(go, "Toggle Active"); go.SetActive(!go.activeSelf);` | Yes |
| Raycast toggle | `Undo.RecordObject(graphic, "Toggle Raycast"); graphic.raycastTarget = !graphic.raycastTarget;` | Yes |
| Solo | Disable all sibling Graphics under same parent, enable only target. Second click restores. | `Undo.SetCurrentGroupName("Solo UI Element")`, record all affected objects. Yes |

### Solo Implementation

- Window holds `SoloState`: `{ bool active, Transform parent, List<(GameObject go, bool wasActive)> saved }`.
- Solo activate: iterate `parent.GetComponentsInChildren<Graphic>(true)`, record and deactivate all GameObjects except target.
- Solo deactivate (or window close, or new selection): restore all saved states.
- **Safety:** `OnDisable` always restores solo state to prevent broken scenes on window close.

### Copy Stack to Clipboard

Bottom of list panel. Serializes filtered list as formatted text:

```
UI Depth Inspector — MainHUD — 42 elements
#00  Background         Raycast:ON   Alpha:1.0
#01  Panel_Main         Raycast:ON   Alpha:1.0
#02  InvisibleBlocker   Raycast:ON   Alpha:0.0  ⚠ INVISIBLE HITBOX
```

Uses `EditorGUIUtility.systemCopyBuffer`.

### Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `F` | Frame selected element (center camera on it) |
| `Escape` | Exit solo mode |

Registered via `ShortcutManager` API.

---

## 9. Performance & Edge Cases

### Performance Target

500 elements. One `GameObject` per element in preview scene with single-quad meshes and shared materials. `PreviewRenderUtility.Render()` completes in <2ms. `ListView` with virtualization handles list scrolling.

### Edge Cases

| Case | Handling |
|------|----------|
| No Canvas in scene | Empty state message in both panes: "No Canvas found in active scene(s)." |
| Canvas with zero Graphic children | Appears in Canvas filter dropdown, empty list. |
| Prefab Mode | `PrefabStageUtility.GetCurrentPrefabStage()` — collect from prefab stage's scene. |
| Multiple loaded scenes (additive) | Collect from all loaded scenes. Canvas filter shows scene origin. |
| Zero-size RectTransform | Emit entry, draw as 1x1 unit dot. Flag "Zero Size" in list. |
| Layout rebuilds | `hierarchyChanged` invalidation handles this. |
| Undo/Redo of external changes | `Undo.undoRedoPerformed` → invalidate cache, rebuild. |

### Explicitly Out of Scope (v1)

- Textured quads (Phase 2 toggle).
- Overdraw / batch-break visualization.
- Layout Group debugging.
- TMP-specific metadata.
- Persistent settings asset.

---

## 10. Open Questions (None)

All design decisions resolved during brainstorming. No TBDs remain.
