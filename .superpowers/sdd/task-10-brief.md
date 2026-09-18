# Task 10 Brief: Final Polish — Mask Wireframes, Frame Selected, Status Bar, README

## Files to Modify
- `Editor/Viewport/UIPreview3DViewport.cs`
- `Editor/UIDepthInspectorWindow.cs`
- `README.md`

## Required Changes

### 1. In `Editor/Viewport/UIPreview3DViewport.cs`
Add mask bounds wireframe rendering to `OnGUI(Rect rect)` after the selection highlight cage:

```csharp
            // Draw mask bounds wireframes
            if (_currentEntries != null)
            {
                Handles.color = new Color(0.25f, 0.88f, 0.25f, 0.8f); // green
                for (int i = 0; i < _previewObjects.Count && i < _currentEntries.Count; i++)
                {
                    var entry = _currentEntries[i];
                    bool hasMask = (entry.Flags & (DiagnosticFlags.HasMask | DiagnosticFlags.HasRectMask2D)) != 0;
                    if (!hasMask) continue;

                    var go = _previewObjects[i];
                    if (go == null) continue;

                    var mf = go.GetComponent<MeshFilter>();
                    if (mf == null || mf.sharedMesh == null) continue;

                    Handles.matrix = go.transform.localToWorldMatrix;
                    Handles.DrawWireCube(mf.sharedMesh.bounds.center, mf.sharedMesh.bounds.size * 1.01f);
                }
                Handles.matrix = Matrix4x4.identity;
            }
```

Add `FrameEntry(int globalDrawIndex)` method:

```csharp
        public void FrameEntry(int globalDrawIndex)
        {
            if (globalDrawIndex < 0 || globalDrawIndex >= _previewObjects.Count) return;
            var go = _previewObjects[globalDrawIndex];
            if (go == null) return;

            _pivotOffset = go.transform.localPosition;
            _zoomDistance = 8f;
        }
```

### 2. In `Editor/UIDepthInspectorWindow.cs`
Add the `F` key FrameSelected shortcut:

```csharp
        [Shortcut("UIDepthInspector/FrameSelected", KeyCode.F)]
        static void FrameSelectedShortcut()
        {
            var wnd = GetWindow<UIDepthInspectorWindow>();
            if (wnd == null) return;

            var selected = Selection.activeGameObject;
            if (selected == null) return;

            var entries = wnd._cache.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Transform != null && entries[i].Transform.gameObject == selected)
                {
                    wnd._viewport.FrameEntry(entries[i].GlobalDrawIndex);
                    wnd._needsRepaint = true;
                    return;
                }
            }
        }
```

### 3. In `README.md`
Update README to mention shortcut keys (`F` to frame selected, `Escape` to exit solo) in the usage section.

## Instructions
1. Apply the modifications to all 3 files.
2. Ensure proper formatting and LF line endings.
3. Stage with `git add Editor/Viewport/UIPreview3DViewport.cs Editor/UIDepthInspectorWindow.cs README.md`.
4. Commit with message: `feat: add mask wireframes, frame-selected shortcut, and final polish`.
5. Write report to `.superpowers/sdd/task-10-report.md`.
