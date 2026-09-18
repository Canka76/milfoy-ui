# Task 5 Brief: Diagnostic Analyzer — Ghost Blockers, Masks, CanvasGroup

## Files to Create
- `Editor/Diagnostics/UIDiagnosticAnalyzer.cs`

## Exact Contents

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIDepthInspector.Editor.Diagnostics
{
    using Core;

    public static class UIDiagnosticAnalyzer
    {
        public static void Analyze(List<UIElementEntry> entries)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.Flags = ComputeFlags(entry);
                entries[i] = entry;
            }
        }

        static DiagnosticFlags ComputeFlags(UIElementEntry entry)
        {
            var flags = DiagnosticFlags.None;

            if (!entry.IsActive)
            {
                flags |= DiagnosticFlags.Inactive;
                return flags;
            }

            // Zero-size check
            if (entry.WorldRect.width < 0.01f || entry.WorldRect.height < 0.01f)
                flags |= DiagnosticFlags.ZeroSize;

            // CanvasGroup overrides
            if (!entry.BlocksRaycasts)
            {
                flags |= DiagnosticFlags.GroupBlocked;
                flags |= DiagnosticFlags.PassiveVisual;
                return flags;
            }

            // Ghost blocker: raycastTarget ON but effectively invisible
            if (entry.RaycastTarget)
            {
                bool isGhost = false;

                // Alpha zero (direct or via CanvasGroup)
                if (entry.EffectiveAlpha <= 0f)
                    isGhost = true;

                // Missing sprite on an Image component
                if (!isGhost && entry.Transform != null &&
                    entry.Transform.TryGetComponent<Image>(out var img) && img.sprite == null)
                    isGhost = true;

                // CanvasGroup making it transparent while still blocking
                if (entry.EffectiveAlpha <= 0f && entry.BlocksRaycasts)
                    flags |= DiagnosticFlags.GroupTransparent;

                if (isGhost)
                    flags |= DiagnosticFlags.GhostBlocker;
                else
                    flags |= DiagnosticFlags.RaycastBlocker;
            }
            else
            {
                flags |= DiagnosticFlags.PassiveVisual;
            }

            // Mask components
            if (entry.Transform != null)
            {
                if (entry.Transform.TryGetComponent<Mask>(out _))
                    flags |= DiagnosticFlags.HasMask;
                if (entry.Transform.TryGetComponent<RectMask2D>(out _))
                    flags |= DiagnosticFlags.HasRectMask2D;
            }

            return flags;
        }
    }
}
```

## Instructions
1. Create directory `Editor/Diagnostics` if it does not exist.
2. Create `Editor/Diagnostics/UIDiagnosticAnalyzer.cs` with the exact C# code specified.
3. Ensure proper formatting and LF line endings.
4. Stage with `git add Editor/Diagnostics/UIDiagnosticAnalyzer.cs`.
5. Commit with message: `feat(diagnostics): add diagnostic analyzer for ghost blockers and mask bounds`.
6. Write report to `.superpowers/sdd/task-5-report.md`.
