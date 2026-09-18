using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UIDepthInspector.Editor.Core;
using UIDepthInspector.Editor.Diagnostics;

namespace UIDepthInspector.Editor.Tests
{
    [InitializeOnLoad]
    public static class AutoTestRunner
    {
        static AutoTestRunner()
        {
            EditorApplication.delayCall += RunAllUnitTests;
        }

        public static void RunAllUnitTests()
        {
            Debug.Log("================ STARTING TDD TEST SUITE ================");
            int passed = 0;
            int failed = 0;

            RunTest("Collect_SingleCanvasWithImages_ReturnsCorrectDrawOrder", Test_DrawOrder, ref passed, ref failed);
            RunTest("Collect_MultipleRootCanvases_SortsBySortingOrderAndLayer", Test_SortingOrder, ref passed, ref failed);
            RunTest("Collect_CanvasGroup_CalculatesCumulativeAlphaAndRaycastBlocking", Test_CanvasGroup, ref passed, ref failed);
            RunTest("Collect_NestedCanvasWithOverrideSorting_CorrectlyDeferredAndOrdered", Test_NestedOverrideSortingCanvas, ref passed, ref failed);
            RunTest("Cache_Invalidate_IncrementsGenerationAndRebuilds", Test_CacheInvalidation, ref passed, ref failed);
            RunTest("Analyze_GhostBlockerWithZeroAlpha_SetsGhostBlockerFlag", Test_GhostBlocker, ref passed, ref failed);
            RunTest("Analyze_PassiveVisual_SetsPassiveVisualFlag", Test_PassiveVisual, ref passed, ref failed);
            RunTest("Analyze_MaskAndRectMask2D_SetsCorrespondingMaskFlags", Test_MaskFlags, ref passed, ref failed);

            Debug.Log($"================ TEST SUMMARY: {passed} PASSED, {failed} FAILED ================");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(failed > 0 ? 1 : 0);
            }
        }

        static void RunTest(string testName, System.Action testAction, ref int passed, ref int failed)
        {
            try
            {
                testAction();
                Debug.Log($"[PASS] {testName}");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] {testName}: {ex.Message}\n{ex.StackTrace}");
                failed++;
            }
        }

        static void Test_DrawOrder()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(GraphicRaycaster));
            try
            {
                var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bgGo.transform.SetParent(canvasGo.transform, false);

                var btnGo = new GameObject("Button", typeof(RectTransform), typeof(Image));
                btnGo.transform.SetParent(canvasGo.transform, false);

                var lblGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
                lblGo.transform.SetParent(btnGo.transform, false);

                var results = UIRenderTreeCollector.Collect();

                if (results.Count != 3) throw new System.Exception($"Expected 3 results, got {results.Count}");
                if (results[0].Name != "Background" || results[0].GlobalDrawIndex != 0) throw new System.Exception("Background failed draw index check");
                if (results[1].Name != "Button" || results[1].GlobalDrawIndex != 1) throw new System.Exception("Button failed draw index check");
                if (results[2].Name != "Label" || results[2].GlobalDrawIndex != 2) throw new System.Exception("Label failed draw index check");
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        static void Test_SortingOrder()
        {
            var canvasAGo = new GameObject("CanvasA", typeof(Canvas));
            var canvasBGo = new GameObject("CanvasB", typeof(Canvas));
            try
            {
                var canvasA = canvasAGo.GetComponent<Canvas>();
                canvasA.sortingOrder = 10;
                var imgA = new GameObject("ImageA", typeof(RectTransform), typeof(Image));
                imgA.transform.SetParent(canvasAGo.transform, false);

                var canvasB = canvasBGo.GetComponent<Canvas>();
                canvasB.sortingOrder = 0;
                var imgB = new GameObject("ImageB", typeof(RectTransform), typeof(Image));
                imgB.transform.SetParent(canvasBGo.transform, false);

                var results = UIRenderTreeCollector.Collect();

                if (results.Count != 2) throw new System.Exception($"Expected 2 results, got {results.Count}");
                if (results[0].Name != "ImageB") throw new System.Exception($"Expected ImageB first, got {results[0].Name}");
                if (results[1].Name != "ImageA") throw new System.Exception($"Expected ImageA second, got {results[1].Name}");
            }
            finally
            {
                Object.DestroyImmediate(canvasAGo);
                Object.DestroyImmediate(canvasBGo);
            }
        }

        static void Test_NestedOverrideSortingCanvas()
        {
            var rootCanvasGo = new GameObject("RootCanvas", typeof(Canvas));
            try
            {
                var rootCanvas = rootCanvasGo.GetComponent<Canvas>();
                rootCanvas.sortingOrder = 0;

                var bgImg = new GameObject("RootBg", typeof(RectTransform), typeof(Image));
                bgImg.transform.SetParent(rootCanvasGo.transform, false);

                // Nested Canvas with overrideSorting and higher sortingOrder
                var nestedCanvasGo = new GameObject("NestedCanvas", typeof(RectTransform), typeof(Canvas));
                nestedCanvasGo.transform.SetParent(rootCanvasGo.transform, false);
                var nestedCanvas = nestedCanvasGo.GetComponent<Canvas>();
                nestedCanvas.overrideSorting = true;
                nestedCanvas.sortingOrder = 100;

                var nestedImg = new GameObject("NestedImg", typeof(RectTransform), typeof(Image));
                nestedImg.transform.SetParent(nestedCanvasGo.transform, false);

                var results = UIRenderTreeCollector.Collect();

                if (results.Count != 2) throw new System.Exception($"Expected 2 results, got {results.Count}");
                if (results[0].Name != "RootBg") throw new System.Exception($"Expected RootBg first, got {results[0].Name}");
                if (results[1].Name != "NestedImg") throw new System.Exception($"Expected NestedImg second, got {results[1].Name}");
                if (results[1].GlobalDrawIndex != 1) throw new System.Exception("NestedImg index incorrect");
            }
            finally
            {
                Object.DestroyImmediate(rootCanvasGo);
            }
        }

        static void Test_CacheInvalidation()
        {
            var cache = new UIRenderTreeCache();
            try
            {
                int gen0 = cache.Generation;
                var entries0 = cache.Entries;
                int gen1 = cache.Generation;

                if (gen1 <= gen0) throw new System.Exception("Generation should increment on first read");

                cache.Invalidate();
                var entries1 = cache.Entries;
                int gen2 = cache.Generation;

                if (gen2 <= gen1) throw new System.Exception("Generation should increment after Invalidate and re-read");
            }
            finally
            {
                cache.Dispose();
            }
        }

        static void Test_CanvasGroup()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas));
            try
            {
                var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(CanvasGroup));
                panelGo.transform.SetParent(canvasGo.transform, false);
                var cg1 = panelGo.GetComponent<CanvasGroup>();
                cg1.alpha = 0.5f;
                cg1.blocksRaycasts = true;

                var subPanelGo = new GameObject("SubPanel", typeof(RectTransform), typeof(CanvasGroup));
                subPanelGo.transform.SetParent(panelGo.transform, false);
                var cg2 = subPanelGo.GetComponent<CanvasGroup>();
                cg2.alpha = 0.5f;
                cg2.blocksRaycasts = false;

                var imgGo = new GameObject("Image", typeof(RectTransform), typeof(Image));
                imgGo.transform.SetParent(subPanelGo.transform, false);
                var img = imgGo.GetComponent<Image>();
                img.color = new Color(1, 1, 1, 1);

                var results = UIRenderTreeCollector.Collect();

                if (results.Count != 1) throw new System.Exception($"Expected 1 result, got {results.Count}");
                if (Mathf.Abs(results[0].EffectiveAlpha - 0.25f) > 0.001f) throw new System.Exception($"Expected alpha 0.25, got {results[0].EffectiveAlpha}");
                if (results[0].BlocksRaycasts) throw new System.Exception("Expected BlocksRaycasts to be false");
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        static void Test_GhostBlocker()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas));
            try
            {
                var imgGo = new GameObject("GhostImage", typeof(RectTransform), typeof(Image));
                imgGo.transform.SetParent(canvasGo.transform, false);

                var img = imgGo.GetComponent<Image>();
                img.color = new Color(1, 1, 1, 0); // alpha = 0
                img.raycastTarget = true;

                var entries = UIRenderTreeCollector.Collect();
                UIDiagnosticAnalyzer.Analyze(entries);

                if (entries.Count != 1) throw new System.Exception($"Expected 1 entry, got {entries.Count}");
                if ((entries[0].Flags & DiagnosticFlags.GhostBlocker) == 0) throw new System.Exception("Expected GhostBlocker flag for alpha=0 with raycastTarget=true");
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        static void Test_PassiveVisual()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas));
            try
            {
                var imgGo = new GameObject("PassiveImage", typeof(RectTransform), typeof(Image));
                imgGo.transform.SetParent(canvasGo.transform, false);

                var img = imgGo.GetComponent<Image>();
                img.color = new Color(1, 1, 1, 1);
                img.raycastTarget = false;

                var entries = UIRenderTreeCollector.Collect();
                UIDiagnosticAnalyzer.Analyze(entries);

                if (entries.Count != 1) throw new System.Exception($"Expected 1 entry, got {entries.Count}");
                if ((entries[0].Flags & DiagnosticFlags.PassiveVisual) == 0) throw new System.Exception("Expected PassiveVisual flag");
                if ((entries[0].Flags & DiagnosticFlags.RaycastBlocker) != 0) throw new System.Exception("RaycastBlocker should not be set");
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        static void Test_MaskFlags()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas));
            try
            {
                var maskGo = new GameObject("MaskObject", typeof(RectTransform), typeof(Image), typeof(Mask));
                maskGo.transform.SetParent(canvasGo.transform, false);

                var rectMaskGo = new GameObject("RectMaskObject", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
                rectMaskGo.transform.SetParent(canvasGo.transform, false);

                var entries = UIRenderTreeCollector.Collect();
                UIDiagnosticAnalyzer.Analyze(entries);

                if (entries.Count != 2) throw new System.Exception($"Expected 2 entries, got {entries.Count}");
                if ((entries[0].Flags & DiagnosticFlags.HasMask) == 0) throw new System.Exception("Expected HasMask flag on first object");
                if ((entries[1].Flags & DiagnosticFlags.HasRectMask2D) == 0) throw new System.Exception("Expected HasRectMask2D flag on second object");
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }
    }
}
