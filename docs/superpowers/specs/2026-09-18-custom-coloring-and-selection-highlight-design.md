# Spec: Custom Color Assignment & High-Readability Selection Highlighting

## Overview
This specification details the architecture and implementation of custom per-element color assignment and high-readability neon selection highlighting for the Milfoy UI Depth Inspector in Unity.

---

## 1. Requirements & Goals

### Custom Color Assignment
- Developers must be able to assign custom diagnostic colors to individual UI hierarchy elements directly from the Editor UI.
- Custom colors must override default category colors in the 3D viewport while preserving diagnostic patterns (such as amber warning stripes for ghost hitboxes).
- Custom color assignments must persist across domain reloads without dirtying scene assets or source control.
- An easy mechanism to reset an element back to its default diagnostic color must be provided.

### Selection Readability
- Selecting an element in either the stack list or 3D viewport must create an unmistakable visual focal point.
- **3D Viewport**: Render a multi-ring neon halo volume cage around the selected 3D slab and softly dim unselected UI slabs to eliminate visual clutter.
- **Stack List Panel**: Emphasize the selected row with a vibrant neon left-accent bar, active background highlight tint, and bold text.

---

## 2. Architecture & Components

```
┌─────────────────────────────────────────────────────────────┐
│                    UICustomColorRegistry                    │
│  (Maps InstanceID -> Color, cached in Unity SessionState)   │
└──────────────────────────────┬──────────────────────────────┘
                               │ Reads/Writes
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                   UIRenderTreeCollector                     │
│  (Populates UIElementEntry.CustomColor from registry)       │
└──────────────────────────────┬──────────────────────────────┘
                               │
            ┌──────────────────┴──────────────────┐
            ▼                                     ▼
┌───────────────────────┐             ┌───────────────────────┐
│   UIStackListPanel    │             │  UIPreview3DViewport  │
│ - Inline ColorPicker  │             │ - MaterialPropertyBlk │
│ - Accent Border & Tint│             │ - Neon Halo Wire Cage │
│ - Reset Context Action│             │ - Layer Dimming Logic │
└───────────────────────┘             └───────────────────────┘
```

---

## 3. Detailed Specifications

### 3.1 `UICustomColorRegistry`
- **Location**: `Assets/UIDepthInspector/Editor/Core/UICustomColorRegistry.cs` (and Package directory).
- **Storage**: In-memory `Dictionary<int, Color>` backed by `SessionState.GetString` / `SessionState.SetString` using lightweight JSON serialization.
- **API**:
  ```csharp
  public static class UICustomColorRegistry
  {
      public static bool TryGetColor(int instanceId, out Color color);
      public static void SetColor(int instanceId, Color color);
      public static void RemoveColor(int instanceId);
      public static void ClearAll();
  }
  ```

### 3.2 `UIElementEntry` Extension
- Add fields:
  ```csharp
  public int InstanceId;
  public Color? CustomColor;
  ```
- Populated in `UIRenderTreeCollector.CollectElements()` from the target GameObject instance ID.

### 3.3 `UIStackListPanel` (UIElements)
- **Inline ColorField**:
  - Placed in `MakeRow()` after the `#index` label and diagnostic dot.
  - Sized compactly (width ~24px, height ~16px) with no alpha channel toggle if unneeded.
  - On `ChangeEvent<Color>`: registers color into `UICustomColorRegistry` and fires `OnFilterChanged` / repaint request.
  - Right-click / ContextClick on row provides "Reset Color" option.
- **Selected Row Styling**:
  - USS selector `.unity-list-view__item--selected` and `.stack-row-container--selected`:
    - `border-left-width: 3px;`
    - `border-left-color: #00E5FF;`
    - `background-color: rgba(0, 229, 255, 0.16);`

### 3.4 `UIPreview3DViewport` (3D IMGUI / PreviewRenderUtility)
- **MaterialPropertyBlock Color Injection**:
  - `PositionQuad` / `CreateQuad`: Uses `MaterialPropertyBlock` with `_Color` property set to `entry.CustomColor ?? GetDefaultColor(entry.Flags)`.
  - When an element is selected (`_highlightIndex >= 0`), apply dimming (`col.a *= 0.6f`) to all unselected quad property blocks.
- **Neon Halo Wire Cage**:
  - Multi-pass `Handles.DrawWireCube` around the selected mesh bounds:
    - Inner bright ring: `Color(0f, 0.9f, 1f, 1f)`, size `1.01f`, width 2.
    - Outer glow ring 1: `Color(0f, 0.9f, 1f, 0.4f)`, size `1.04f`.
    - Outer glow ring 2: `Color(0f, 0.9f, 1f, 0.15f)`, size `1.08f`.

---

## 4. Testing & Verification

1. **Unit Tests (`AutoTestRunner.cs`)**:
   - `Test_CustomColorRegistry_Persistence()`: Verify set, get, remove, and JSON string session round-trip.
   - `Test_UIElementEntry_CustomColorMapping()`: Verify `UIRenderTreeCollector` accurately attaches custom colors to matching GameObject entries.
2. **Batchmode & Editor Tests**:
   - Execute test harness via Unity Batchmode.
   - Verify live interaction in Unity Editor console and viewport.
