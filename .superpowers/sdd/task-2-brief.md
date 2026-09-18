# Task 2 Brief: UIElementEntry Data Struct & Diagnostic Flags

## Files to Create
- `Editor/Core/UIElementEntry.cs`

## Exact Contents

```csharp
using System;
using UnityEngine;

namespace UIDepthInspector.Editor.Core
{
    [Flags]
    public enum DiagnosticFlags
    {
        None             = 0,
        RaycastBlocker   = 1 << 0,
        GhostBlocker     = 1 << 1,
        PassiveVisual    = 1 << 2,
        Inactive         = 1 << 3,
        GroupBlocked     = 1 << 4,
        GroupTransparent = 1 << 5,
        HasMask          = 1 << 6,
        HasRectMask2D    = 1 << 7,
        ZeroSize         = 1 << 8,
    }

    public struct UIElementEntry
    {
        public int GlobalDrawIndex;
        public string Name;
        public bool IsActive;
        public bool RaycastTarget;
        public float EffectiveAlpha;
        public bool BlocksRaycasts;
        public Rect WorldRect;
        public RenderMode CanvasRenderMode;
        public int RootCanvasId;
        public string RootCanvasName;
        public DiagnosticFlags Flags;
        public Transform Transform;
    }
}
```

## Instructions
1. Create directory `Editor/Core` if it does not exist.
2. Create `Editor/Core/UIElementEntry.cs` with the exact namespace `UIDepthInspector.Editor.Core` and exact types `DiagnosticFlags` (enum) and `UIElementEntry` (struct).
3. Commit with message: `feat(core): add UIElementEntry struct and DiagnosticFlags enum`
4. Write report to `.superpowers/sdd/task-2-report.md`.
