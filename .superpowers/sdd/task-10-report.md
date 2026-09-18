# Task 10 Implementation Report: Final Polish — Mask Wireframes, Frame Selected, Status Bar, README

## Status: DONE
**Commit Hash**: `aa5ac0793760ae5fe3e9dbe725c89a5ce2174825`

## Summary of Changes

1. **`Editor/Viewport/UIPreview3DViewport.cs`**:
   - Added `FrameEntry(int globalDrawIndex)` method which centers `_pivotOffset` on the target element's local position and resets zoom distance to 8 units.
   - Added mask bounds wireframe rendering in `OnGUI(Rect rect)` using green `Handles.DrawWireCube` for elements flagged with `DiagnosticFlags.HasMask` or `DiagnosticFlags.HasRectMask2D`.

2. **`Editor/UIDepthInspectorWindow.cs`**:
   - Added `[Shortcut("UIDepthInspector/FrameSelected", KeyCode.F)]` shortcut handler to focus the 3D viewport on the currently selected GameObject in the hierarchy.

3. **`README.md`**:
   - Documented viewport navigation, mouse controls, and keyboard shortcuts (`F` to frame selected element, `Escape` to exit solo isolation).
