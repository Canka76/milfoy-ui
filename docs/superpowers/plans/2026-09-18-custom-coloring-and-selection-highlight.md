# Custom Color Assignment & High-Readability Selection Highlighting Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Allow users to assign custom diagnostic colors to UI hierarchy elements with session persistence and render high-contrast neon halo selection cages with unselected layer dimming in both 3D Viewport and Stack List.

**Architecture:** A session-backed `UICustomColorRegistry` stores custom color overrides. `UIRenderTreeCollector` maps these onto `UIElementEntry`. `UIStackListPanel` displays inline color pickers and neon selection rows. `UIPreview3DViewport` applies custom colors via `MaterialPropertyBlock`, renders multi-pass neon halo wire cages, and dims unselected layers.

**Tech Stack:** Unity 6 C#, UIElements (UI Toolkit), PreviewRenderUtility / IMGUI Handles, ShaderLab / HLSL.

## Global Constraints
- Target Unity 6000.7+
- Dual sync across `Assets/UIDepthInspector/` and `Packages/com.openupm.ui-depth-inspector/`
- Zero per-frame allocations during 3D preview render loops
- Non-destructive: scene files are never dirtied by custom coloring

---

### Task 1: UICustomColorRegistry & UIElementEntry Extension

**Files:**
- Create: `Assets/UIDepthInspector/Editor/Core/UICustomColorRegistry.cs`
- Create: `Packages/com.openupm.ui-depth-inspector/Editor/Core/UICustomColorRegistry.cs`
- Modify: `Assets/UIDepthInspector/Editor/Core/UIElementEntry.cs`
- Modify: `Packages/com.openupm.ui-depth-inspector/Editor/Core/UIElementEntry.cs`
- Modify: `Assets/UIDepthInspector/Editor/Core/UIRenderTreeCollector.cs`
- Modify: `Packages/com.openupm.ui-depth-inspector/Editor/Core/UIRenderTreeCollector.cs`
- Test: `Assets/UIDepthInspector/Tests/Editor/AutoTestRunner.cs`

**Interfaces:**
- Produces:
  - `UICustomColorRegistry.TryGetColor(int instanceId, out Color color)`
  - `UICustomColorRegistry.SetColor(int instanceId, Color color)`
  - `UICustomColorRegistry.RemoveColor(int instanceId)`
  - `UICustomColorRegistry.ClearAll()`
  - `UIElementEntry.InstanceId` (int)
  - `UIElementEntry.CustomColor` (Color?)

- [ ] **Step 1: Write the failing unit tests in `AutoTestRunner.cs`**
Add `Test_CustomColorRegistry()` to verify set, get, remove, and serialization.

- [ ] **Step 2: Implement `UICustomColorRegistry.cs`**
Create dictionary backed by `SessionState` JSON serialization.

- [ ] **Step 3: Update `UIElementEntry.cs` and `UIRenderTreeCollector.cs`**
Add `InstanceId` and `CustomColor` to `UIElementEntry`, populate in `UIRenderTreeCollector`.

- [ ] **Step 4: Sync to `Packages/com.openupm.ui-depth-inspector/`**
Mirror changes to the UPM package directory.

---

### Task 2: UIStackListPanel Inline Color Picker & Selection Accent

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Panels/UIStackListPanel.cs`
- Modify: `Packages/com.openupm.ui-depth-inspector/Editor/Panels/UIStackListPanel.cs`
- Modify: `Assets/UIDepthInspector/Editor/Resources/UIDepthInspector.uss`
- Modify: `Packages/com.openupm.ui-depth-inspector/Editor/Resources/UIDepthInspector.uss`

**Interfaces:**
- Consumes: `UICustomColorRegistry`, `UIElementEntry.CustomColor`, `UIElementEntry.InstanceId`
- Produces: Inline `ColorField` per row, context click reset, `.stack-row-container--selected` USS styling.

- [ ] **Step 1: Add compact `ColorField` to `MakeRow()` in `UIStackListPanel.cs`**
Add a miniature color swatch button / `ColorField` between index and name.

- [ ] **Step 2: Bind color and change event in `BindRow()`**
Register color callback to update `UICustomColorRegistry` and invoke filter/repaint.

- [ ] **Step 3: Add right-click context menu to reset custom color**
Add contextual menu action to remove custom color.

- [ ] **Step 4: Update USS with high-contrast neon selection styles**
Add `.unity-list-view__item--selected` cyan left border and highlighted background tint.

- [ ] **Step 5: Sync to `Packages/com.openupm.ui-depth-inspector/`**

---

### Task 3: UIPreview3DViewport MaterialPropertyBlock Tinting & Neon Halo Selection Cage

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Viewport/UIPreview3DViewport.cs`
- Modify: `Packages/com.openupm.ui-depth-inspector/Editor/Viewport/UIPreview3DViewport.cs`

**Interfaces:**
- Consumes: `UIElementEntry.CustomColor`, `UIElementEntry.InstanceId`, `_highlightIndex`
- Produces: `MaterialPropertyBlock` per-quad color override with unselected dimming, multi-ring neon halo cage.

- [ ] **Step 1: Update `PositionQuad` / `CreateQuad` with `MaterialPropertyBlock`**
Pass custom color or default diagnostic color with dynamic dimming (`0.6f` alpha for unselected items when selection is active).

- [ ] **Step 2: Enhance `DrawWireframeCage` with multi-ring neon halo**
Implement sharp inner cyan core (`Color(0, 0.9f, 1f, 1f)`) + two expanded soft glow rings.

- [ ] **Step 3: Sync to `Packages/com.openupm.ui-depth-inspector/`**

---

### Task 4: Automated Testing & Verification

**Files:**
- Modify: `Assets/UIDepthInspector/Tests/Editor/AutoTestRunner.cs`
- Modify: `Packages/com.openupm.ui-depth-inspector/Tests/Editor/AutoTestRunner.cs`

- [ ] **Step 1: Run automated unit tests in `AutoTestRunner.cs`**
Verify all tests pass cleanly.

- [ ] **Step 2: Verify live Editor logs and UI behavior**
Check Unity Editor log stream and confirm zero exceptions.
