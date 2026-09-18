# Task 3 Report: Render Tree Collector — Exact uGUI Draw Order Traversal

## Status: DONE

## Implemented Files
- `Editor/Core/UIRenderTreeCollector.cs`

## Commit
- Hash: `92b135bc470ea65b1e7ec173e0c8cb2f66374ac8`
- Message: `feat(core): implement render tree collector with exact uGUI draw order`

## Key Implementation Details
1. **Zero Allocations in Static Caches:**
   - Static lists `s_RootCanvases`, `s_Result`, `s_SortedRoots`, and `s_DeferredSubtrees` are reused and cleared on each `Collect()` invocation.
2. **Exact uGUI Hierarchy and Sorting Order Traversal:**
   - Discovers all root canvases across loaded scenes using `Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)`.
   - Sorts root canvases primary by sorting layer value (`SortingLayer.GetLayerValueFromID`), secondary by `sortingOrder`.
   - Traverses Canvas hierarchies in DFS sibling order.
   - Accurately tracks cumulative `CanvasGroup.alpha` and `CanvasGroup.blocksRaycasts`.
   - Handles nested `Canvas` instances with `overrideSorting = true` by deferring their subtree traversal and appending them in sorted order.
   - Accurately calculates `WorldRect` using `RectTransformUtility.CalculateRelativeRectTransformBounds` and transformed center.
   - Re-indexes all entries with the final global draw order indices.
