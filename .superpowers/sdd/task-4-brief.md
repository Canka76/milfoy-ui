# Task 4 Brief: Render Tree Cache — Dirty-Check Wrapper

## Files to Create
- `Editor/Core/UIRenderTreeCache.cs`

## Exact Contents

```csharp
using System.Collections.Generic;
using UnityEditor;

namespace UIDepthInspector.Editor.Core
{
    public class UIRenderTreeCache
    {
        List<UIElementEntry> _entries = new();
        int _generation;
        bool _dirty = true;

        public IReadOnlyList<UIElementEntry> Entries
        {
            get
            {
                if (_dirty)
                    Rebuild();
                return _entries;
            }
        }

        public int Generation => _generation;

        public UIRenderTreeCache()
        {
            EditorApplication.hierarchyChanged += Invalidate;
            Undo.undoRedoPerformed += Invalidate;
        }

        public void Dispose()
        {
            EditorApplication.hierarchyChanged -= Invalidate;
            Undo.undoRedoPerformed -= Invalidate;
        }

        public void Invalidate()
        {
            _dirty = true;
        }

        void Rebuild()
        {
            _entries = UIRenderTreeCollector.Collect();
            _generation++;
            _dirty = false;
        }
    }
}
```

## Instructions
1. Create `Editor/Core/UIRenderTreeCache.cs` with the exact C# code specified.
2. Ensure proper formatting and LF line endings.
3. Stage with `git add Editor/Core/UIRenderTreeCache.cs`.
4. Commit with message: `feat(core): add render tree cache with dirty-check invalidation`.
5. Write report to `.superpowers/sdd/task-4-report.md`.
