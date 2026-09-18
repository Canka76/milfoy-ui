# 3D Viewport Depth Direction Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Correct the 3D viewport Z-offset direction so higher draw order / foreground elements stack forward toward the camera (-Z direction) instead of backward into the screen (+Z direction).

**Architecture:** Update `UIPreview3DViewport.PositionQuad` to place higher index layers at negative Z offsets relative to the $Z=0$ root canvas base. Add comprehensive Editor unit tests validating quad position Z-ordering and raycast selection ordering.

**Tech Stack:** Unity 6 C# (Unity Editor, UIElements, PreviewRenderUtility, NUnit).

## Global Constraints
- Target file: `Assets/UIDepthInspector/Editor/Viewport/UIPreview3DViewport.cs`
- Tests file: `Assets/UIDepthInspector/Tests/Editor/UIPreview3DViewportTests.cs`
- Zero GC allocations during continuous viewport updates.
- Keep coordinate normalization within bounds $[-4, +4]$ in X/Y.

---

### Task 1: Viewport Layer Z-Offset Inversion & Ordering Tests

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Viewport/UIPreview3DViewport.cs:356-369`
- Create: `Assets/UIDepthInspector/Tests/Editor/UIPreview3DViewportTests.cs`

**Interfaces:**
- Consumes: `UIPreview3DViewport.PositionQuad(Transform t, UIElementEntry entry, int index, int totalCount)`
- Produces: Negative Z layer positioning where $Z_{\text{index}} \le Z_{\text{index-1}}$, ensuring foreground elements are closer to camera at $(0, 0, -10)$.

- [ ] **Step 1: Write the unit test for viewport quad depth placement**

```csharp
using NUnit.Framework;
using UnityEngine;
using UIDepthInspector.Editor.Core;
using UIDepthInspector.Editor.Viewport;
using System.Collections.Generic;

namespace UIDepthInspector.Editor.Tests
{
    public class UIPreview3DViewportTests
    {
        [Test]
        public void HigherDrawIndex_IsPlacedCloserToCameraAlongNegativeZ()
        {
            var viewport = new UIPreview3DViewport();
            viewport.SetExplosionFactor(5f);

            var entry0 = new UIElementEntry { GlobalDrawIndex = 0, WorldRect = new Rect(0, 0, 100, 100) };
            var entry1 = new UIElementEntry { GlobalDrawIndex = 1, WorldRect = new Rect(0, 0, 100, 100) };

            var entries = new List<UIElementEntry> { entry0, entry1 };
            viewport.SetEntries(entries);

            // Access preview objects via reflection or verify layer Z positions
            var field = typeof(UIPreview3DViewport).GetField("_previewObjects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var previewObjects = (List<GameObject>)field.GetValue(viewport);

            Assert.IsNotNull(previewObjects);
            Assert.AreEqual(2, previewObjects.Count);

            float z0 = previewObjects[0].transform.localPosition.z;
            float z1 = previewObjects[1].transform.localPosition.z;

            // z1 (higher draw order / foreground) should be strictly less than z0 (closer to camera along -Z)
            Assert.Less(z1, z0, "Higher draw order layer must be closer to camera along negative Z");

            viewport.Dispose();
        }
    }
}
```

- [ ] **Step 2: Update PositionQuad in UIPreview3DViewport.cs**

In `Assets/UIDepthInspector/Editor/Viewport/UIPreview3DViewport.cs`, change line 360:
```csharp
void PositionQuad(Transform t, UIElementEntry entry, int index, int totalCount)
{
    float normX = (entry.WorldRect.center.x - _normalizationRect.x) / _normalizationRect.width * 8f - 4f;
    float normY = (entry.WorldRect.center.y - _normalizationRect.y) / _normalizationRect.height * 8f - 4f;
    float z = totalCount > 0 ? -index * (_explosionFactor / Mathf.Max(totalCount, 1)) : 0f;

    t.localPosition = new Vector3(normX, normY, z);

    float scaleX = Mathf.Max(entry.WorldRect.width / _normalizationRect.width * 8f, 0.05f);
    float scaleY = Mathf.Max(entry.WorldRect.height / _normalizationRect.height * 8f, 0.05f);
    float scaleZ = Mathf.Max(_slabThickness, 0.01f);

    t.localScale = new Vector3(scaleX, scaleY, scaleZ);
}
```

- [ ] **Step 3: Run the test suite via Unity batchmode**

```bash
"C:/Program Files/Unity/Hub/Editor/6000.7.0a2/Editor/Unity.exe" -batchmode -quit -projectPath "D:/Unity/UI_Window" -executeMethod UIDepthInspector.Editor.Tests.AutoTestRunner.RunAllTests
```

- [ ] **Step 4: Commit changes**

```bash
git add Assets/UIDepthInspector/Editor/Viewport/UIPreview3DViewport.cs Assets/UIDepthInspector/Tests/Editor/UIPreview3DViewportTests.cs
git commit -m "fix(viewport): invert quad z-offset so higher draw order layers stack forward toward camera"
```
