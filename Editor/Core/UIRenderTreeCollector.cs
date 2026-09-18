using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIDepthInspector.Editor.Core
{
    public static class UIRenderTreeCollector
    {
        // Reusable lists — no per-call allocation
        static readonly List<Canvas> s_RootCanvases = new();
        static readonly List<UIElementEntry> s_Result = new();
        static readonly List<(int sortKey, int sortOrder, Canvas canvas)> s_SortedRoots = new();
        static readonly List<(int sortKey, int sortOrder, Transform root, int insertIndex)> s_DeferredSubtrees = new();

        /// <summary>
        /// Collect from all loaded scenes.
        /// </summary>
        public static List<UIElementEntry> Collect()
        {
            s_Result.Clear();
            s_RootCanvases.Clear();
            s_SortedRoots.Clear();
            s_DeferredSubtrees.Clear();

            // Gather root canvases from all loaded scenes
            var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in allCanvases)
            {
                if (c.isRootCanvas)
                    s_RootCanvases.Add(c);
            }

            // Sort by sorting layer value, then sorting order
            s_SortedRoots.Clear();
            foreach (var c in s_RootCanvases)
            {
                int layerValue = SortingLayer.GetLayerValueFromID(c.sortingLayerID);
                s_SortedRoots.Add((layerValue, c.sortingOrder, c));
            }
            s_SortedRoots.Sort((a, b) =>
            {
                int cmp = a.sortKey.CompareTo(b.sortKey);
                return cmp != 0 ? cmp : a.sortOrder.CompareTo(b.sortOrder);
            });

            int globalIndex = 0;

            foreach (var (_, _, rootCanvas) in s_SortedRoots)
            {
                TraverseCanvas(rootCanvas.transform, rootCanvas, 1f, true, ref globalIndex);
            }

            // Handle deferred override-sorting subtrees
            if (s_DeferredSubtrees.Count > 0)
            {
                // Sort deferred subtrees by their sort keys
                s_DeferredSubtrees.Sort((a, b) =>
                {
                    int cmp = a.sortKey.CompareTo(b.sortKey);
                    return cmp != 0 ? cmp : a.sortOrder.CompareTo(b.sortOrder);
                });

                // Collect deferred entries into a temp list, then merge
                var deferredEntries = new List<UIElementEntry>();
                foreach (var (_, _, root, _) in s_DeferredSubtrees)
                {
                    var nestedCanvas = root.GetComponent<Canvas>();
                    float parentAlpha = GetCumulativeCanvasGroupAlpha(root.parent);
                    bool parentBlocksRaycasts = GetCumulativeBlocksRaycasts(root.parent);
                    int tempIndex = globalIndex;
                    int countBefore = s_Result.Count;

                    TraverseCanvas(root, nestedCanvas != null ? nestedCanvas : root.GetComponentInParent<Canvas>(),
                        parentAlpha, parentBlocksRaycasts, ref tempIndex);

                    // Move newly added entries to deferred list
                    for (int i = countBefore; i < s_Result.Count; i++)
                        deferredEntries.Add(s_Result[i]);
                    s_Result.RemoveRange(countBefore, s_Result.Count - countBefore);

                    globalIndex = tempIndex;
                }

                // Append deferred entries (already in sort order)
                s_Result.AddRange(deferredEntries);
            }

            // Re-index all entries with final global draw order
            for (int i = 0; i < s_Result.Count; i++)
            {
                var entry = s_Result[i];
                entry.GlobalDrawIndex = i;
                s_Result[i] = entry;
            }

            return s_Result;
        }

        static void TraverseCanvas(Transform root, Canvas rootCanvas,
            float parentAlpha, bool parentBlocksRaycasts, ref int globalIndex)
        {
            TraverseRecursive(root, rootCanvas, parentAlpha, parentBlocksRaycasts, ref globalIndex);
        }

        static void TraverseRecursive(Transform current, Canvas rootCanvas,
            float cumulativeAlpha, bool cumulativeBlocksRaycasts, ref int globalIndex)
        {
            // Check for CanvasGroup on this node
            if (current.TryGetComponent<CanvasGroup>(out var group))
            {
                cumulativeAlpha *= group.alpha;
                cumulativeBlocksRaycasts = cumulativeBlocksRaycasts && group.blocksRaycasts;
            }

            // Check for nested Canvas with overrideSorting — defer it
            if (current != rootCanvas.transform && current.TryGetComponent<Canvas>(out var nestedCanvas))
            {
                if (nestedCanvas.overrideSorting)
                {
                    int layerValue = SortingLayer.GetLayerValueFromID(nestedCanvas.sortingLayerID);
                    s_DeferredSubtrees.Add((layerValue, nestedCanvas.sortingOrder, current, s_Result.Count));
                    return; // Skip this subtree for now
                }
            }

            // Emit entry for any Graphic on this node
            if (current.TryGetComponent<Graphic>(out var graphic))
            {
                var rt = current as RectTransform ?? current.GetComponent<RectTransform>();
                Rect worldRect = Rect.zero;
                if (rt != null)
                {
                    var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rt);
                    var center = rt.TransformPoint(bounds.center);
                    worldRect = new Rect(
                        center.x - bounds.extents.x,
                        center.y - bounds.extents.y,
                        bounds.size.x,
                        bounds.size.y
                    );
                }

                float effectiveAlpha = graphic.color.a * cumulativeAlpha;

                s_Result.Add(new UIElementEntry
                {
                    GlobalDrawIndex = globalIndex++,
                    Name = current.gameObject.name,
                    IsActive = current.gameObject.activeInHierarchy,
                    RaycastTarget = graphic.raycastTarget,
                    EffectiveAlpha = effectiveAlpha,
                    BlocksRaycasts = cumulativeBlocksRaycasts,
                    WorldRect = worldRect,
                    CanvasRenderMode = rootCanvas.renderMode,
                    RootCanvasId = rootCanvas.GetInstanceID(),
                    RootCanvasName = rootCanvas.gameObject.name,
                    Flags = DiagnosticFlags.None, // computed by UIDiagnosticAnalyzer
                    Transform = current,
                });
            }

            // Recurse children in sibling order
            for (int i = 0; i < current.childCount; i++)
            {
                TraverseRecursive(current.GetChild(i), rootCanvas,
                    cumulativeAlpha, cumulativeBlocksRaycasts, ref globalIndex);
            }
        }

        static float GetCumulativeCanvasGroupAlpha(Transform t)
        {
            float alpha = 1f;
            while (t != null)
            {
                if (t.TryGetComponent<CanvasGroup>(out var cg))
                    alpha *= cg.alpha;
                t = t.parent;
            }
            return alpha;
        }

        static bool GetCumulativeBlocksRaycasts(Transform t)
        {
            bool blocks = true;
            while (t != null)
            {
                if (t.TryGetComponent<CanvasGroup>(out var cg))
                    blocks = blocks && cg.blocksRaycasts;
                t = t.parent;
            }
            return blocks;
        }
    }
}
