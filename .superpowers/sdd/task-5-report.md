# Task 5 Report: Diagnostic Analyzer — Ghost Blockers, Masks, CanvasGroup

## Implementation Summary
- Created `Editor/Diagnostics/UIDiagnosticAnalyzer.cs` according to the task brief.
- Implemented static `Analyze(List<UIElementEntry> entries)` method and `ComputeFlags(UIElementEntry entry)` routine.
- Properly handles:
  - Inactive UI elements (`DiagnosticFlags.Inactive`).
  - Zero-size RectTransforms (`DiagnosticFlags.ZeroSize`).
  - CanvasGroup raycast blocking overrides (`DiagnosticFlags.GroupBlocked`, `DiagnosticFlags.PassiveVisual`).
  - Ghost blocker identification (alpha <= 0, missing sprite on Image with raycastTarget enabled).
  - CanvasGroup transparency while raycast blocking (`DiagnosticFlags.GroupTransparent`).
  - Standard raycast blockers vs passive visuals.
  - Mask detection (`Mask` -> `DiagnosticFlags.HasMask`, `RectMask2D` -> `DiagnosticFlags.HasRectMask2D`).
- File formatted with LF line endings.

## Git Commit
- **Commit**: `844aa3c53631b085490718dd163e2f95f18c9703`
- **Message**: `feat(diagnostics): add diagnostic analyzer for ghost blockers and mask bounds`
- **Changed Files**: `Editor/Diagnostics/UIDiagnosticAnalyzer.cs`

## Status
DONE
