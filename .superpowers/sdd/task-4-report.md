# Task 4 Report: Render Tree Cache — Dirty-Check Wrapper

## Status
- **Status:** DONE
- **Commit:** `e861ffa0d92cec2963393c7b93b0659e2ea4bae9`
- **File Created:** `Editor/Core/UIRenderTreeCache.cs`

## Summary of Implementation
- Implemented `UIRenderTreeCache` under namespace `UIDepthInspector.Editor.Core`.
- Added lazy rebuild dirty-check mechanism via `Entries` property.
- Added generation counter incrementing on each rebuild.
- Subscribed to `EditorApplication.hierarchyChanged` and `Undo.undoRedoPerformed` in constructor for automatic cache invalidation.
- Implemented `Dispose()` to properly unsubscribe from static editor events to prevent memory leaks.
- Implemented explicit `Invalidate()` method.
- File created with LF line endings and committed cleanly.
