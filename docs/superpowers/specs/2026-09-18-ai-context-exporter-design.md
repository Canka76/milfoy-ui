# AI-Friendly UI Context Exporter & CLI Diagnostics Design Spec

## 1. Overview & Motivation
When AI agents or developers debug Unity UI issues (such as invisible ghost blockers preventing button clicks, z-order draw inversions, zero-size layout bugs, or batch-breaking material interleaving), feeding raw scene files, prefab YAML, or entire source files wastes thousands of tokens and misses runtime layout state.

This system provides **token-dense, actionable UI diagnostic summaries** in both human-readable Markdown (<500 tokens) and structured JSON, accessible directly via:
1. An **in-editor one-click clipboard exporter** on the UI Depth Inspector toolbar.
2. A **headless CLI batchmode entry point** (`UIDepthInspectorCLI.DumpContext`) for automated AI coding harnesses.

---

## 2. Architecture & Components

```
+-------------------------------------------------------------+
|               Unity UI Hierarchy / Canvas                   |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|     UIRenderTreeCollector & UIDiagnosticAnalyzer (Core)     |
|   (Extracts draw order, flags, bounds, alpha, raycasts)     |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|         UIAIContextExporter (New Export Subsystem)          |
|  - ExportToCompactMarkdown(entries, canvas) -> Token Dense  |
|  - ExportToJson(entries, canvas, verbose) -> Machine Read   |
+-------------------------------------------------------------+
           |                                       |
           v                                       v
+---------------------+                 +---------------------+
| UIToolbarPanel      |                 | UIDepthInspectorCLI |
| (GUI Clipboard Copy)|                 | (Headless Batchmode)|
+---------------------+                 +---------------------+
```

### Component Details
1. **`UIAIContextExporter` (`UIDepthInspector.Editor.Export`)**:
   - Pure formatting and diagnostic summary engine.
   - Converts `List<UIElementEntry>` and canvas metadata into token-optimized text or structured JSON.
   - Calculates specific remediation advice for detected flags (`GhostBlocker`, `ZeroSize`, `GroupTransparent`, `Inactive`).

2. **`UIToolbarPanel` Integration (`UIDepthInspector.Editor.Panels`)**:
   - Adds a `Copy AI Prompt` button to the toolbar.
   - Left-click: Copies Compact Markdown report to `EditorGUIUtility.systemCopyBuffer` and shows a temporary status feedback in the window.
   - Shift+click / Context Menu: Copies Deep JSON.

3. **`UIDepthInspectorCLI` (`UIDepthInspector.Editor.Export`)**:
   - Static batch method `DumpContext()` executable via Unity CLI:
     `-executeMethod UIDepthInspector.Editor.Export.UIDepthInspectorCLI.DumpContext -dumpFormat [md|json] -dumpOutput [path]`
   - If no `-dumpOutput` is specified, writes the formatted text to standard Unity debug console/stdout with demarcated markers (`=== BEGIN UI AI CONTEXT ===` ... `=== END UI AI CONTEXT ===`).

---

## 3. Data Formats & Token Optimization

### Compact Markdown Format (Default, ~250–500 tokens)
```markdown
# UI Depth Inspector - AI Diagnostic Report
**Canvas**: GameCanvas (ScreenSpaceOverlay) | Total Elements: 8 | Draw Range: [0..7]

## Hierarchy & Draw Order
- [#00] GameCanvas (Canvas) [PASSIVE]
  └── [#01] Background (Image) [RAYCAST] [GHOST_BLOCKER] (Alpha: 0.0, raycastTarget: true)
  └── [#02] ContentPanel (RectTransform)
      ├── [#03] HeaderText (TextMeshProUGUI) [PASSIVE]
      ├── [#04] PlayButton (Button/Image) [RAYCAST]
      │   └── [#05] PlayText (TextMeshProUGUI) [RAYCAST] (Redundant raycast target)
      └── [#06] PopupOverlay (CanvasGroup) [GROUP_BLOCKED] (alpha: 0.0, blocksRaycasts: false)
          └── [#07] InvisibleHitbox (Image) [ZERO_SIZE] (0x0 rect)

## Critical Diagnostic Anomalies (3)
1. ⚠️ **[GHOST_BLOCKER]** `Background` (DrawIndex: 1)
   - **Reason**: `raycastTarget` is enabled, but image is fully transparent (EffectiveAlpha = 0). It intercepts clicks before reaching underlying UI.
   - **Fix**: Disable `Raycast Target` on `Background` or remove Graphic.
2. ⚠️ **[REDUNDANT_RAYCAST]** `PlayButton/PlayText` (DrawIndex: 5)
   - **Reason**: Text inside button has `raycastTarget` enabled; parent button already handles clicks.
   - **Fix**: Disable `Raycast Target` on child text component.
3. ⚠️ **[ZERO_SIZE]** `PopupOverlay/InvisibleHitbox` (DrawIndex: 7)
   - **Reason**: RectTransform size is 0x0 with raycastTarget on.
   - **Fix**: Adjust sizeDelta or disable raycastTarget.
```

### JSON Format (Deep/Machine-readable)
```json
{
  "timestamp": "2026-09-18T14:15:00Z",
  "canvas": {
    "name": "GameCanvas",
    "renderMode": "ScreenSpaceOverlay",
    "sortingOrder": 0
  },
  "elementCount": 8,
  "elements": [
    {
      "drawIndex": 1,
      "name": "Background",
      "path": "GameCanvas/Background",
      "type": "Image",
      "rect": { "x": 0, "y": 0, "width": 1920, "height": 1080 },
      "effectiveAlpha": 0.0,
      "raycastTarget": true,
      "flags": ["GhostBlocker", "RaycastBlocker"],
      "hasMask": false
    }
  ],
  "anomalies": [
    {
      "severity": "Warning",
      "elementName": "Background",
      "drawIndex": 1,
      "flag": "GhostBlocker",
      "description": "Graphic has raycastTarget enabled but effective alpha is 0.",
      "suggestedFix": "Disable raycastTarget on 'Background'."
    }
  ]
}
```

---

## 4. Verification & Testing Plan
1. **Unit & Integration Tests (`UIAIContextExporterTests.cs`)**:
   - Test Markdown formatting generates expected structure and indentation.
   - Test JSON exporter serialization and schema compliance.
   - Test anomaly detection and suggestion generation for Ghost Blockers, Zero Size elements, and Redundant Raycasts.
   - Test headless CLI parameter parsing (`-dumpFormat`, `-dumpOutput`).
2. **Editor Compilation & Clean Domain Reload**:
   - Verify `UIDepthInspector.Editor.dll` compiles cleanly with zero warnings/errors.
3. **Batchmode Execution Verification**:
   - Run automated test runner via Unity batchmode to verify all tests pass.
