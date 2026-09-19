using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UIDepthInspector.Editor.Core;
using UIDepthInspector.Editor.Diagnostics;
using UIDepthInspector.Editor.Export;
using UIDepthInspector.Editor.Viewport;
namespace UIDepthInspector.Editor.Tests
{
    public static class AutoTestRunner
    {
        [MenuItem("Tools/Run UI Depth Tests")]
        public static void RunAllTests() => RunAllUnitTests();

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
            RunTest("CustomColorRegistry_Persistence", Test_CustomColorRegistry_Persistence, ref passed, ref failed);
            RunTest("UIElementEntry_CustomColor", Test_UIElementEntry_CustomColor, ref passed, ref failed);
            RunTest("Viewport_HigherDrawIndex_IsPlacedCloserToCameraAlongNegativeZ", Test_ViewportDepthPlacement, ref passed, ref failed);
            RunTest("Viewport_RenderQueue_IncrementsPerLayer_GuaranteesDrawOrder", Test_ViewportRenderQueueOrdering, ref passed, ref failed);
            RunTest("Analyze_SpatialOcclusion_SetsOcclusionBlockerAndPopulatesPairs", Test_SpatialOcclusion, ref passed, ref failed);
            RunTest("Analyze_NestedLabelRaycast_SetsNestedLabelRaycastFlag", Test_NestedLabelRaycast, ref passed, ref failed);
            RunTest("Exporter_Markdown_AnomaliesMode_OutputsOnlyAnomaliesAndSpatialBlocks", Test_ExporterMarkdownAnomalies, ref passed, ref failed);
            RunTest("Exporter_Json_AnomaliesMode_IncludesStructuredFixActions", Test_ExporterJsonAnomalies, ref passed, ref failed);
            RunTest("CLI_ParseExportMode_CorrectlyResolvesAllModesAndDefaults", Test_CLI_ParseExportMode, ref passed, ref failed);
            RunTest("AutoExporter_ExportActiveContext_GeneratesAnomaliesContextFiles", Test_AutoExporter, ref passed, ref failed);
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

                var results = UIRenderTreeCollector.Collect(new[] { canvasGo.GetComponent<Canvas>() });

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
                var results = UIRenderTreeCollector.Collect(new[] { canvasA, canvasB });

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

                var results = UIRenderTreeCollector.Collect(new[] { rootCanvas });

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
                var results = UIRenderTreeCollector.Collect(new[] { canvasGo.GetComponent<Canvas>() });

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

                var entries = UIRenderTreeCollector.Collect(new[] { canvasGo.GetComponent<Canvas>() });
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

                var entries = UIRenderTreeCollector.Collect(new[] { canvasGo.GetComponent<Canvas>() });
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

                var entries = UIRenderTreeCollector.Collect(new[] { canvasGo.GetComponent<Canvas>() });
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

        static void Test_CustomColorRegistry_Persistence()
        {
            UICustomColorRegistry.ClearAll();
            try
            {
                int testId1 = 12345;
                int testId2 = 67890;
                var color1 = new Color(1f, 0.5f, 0.25f, 1f);
                var color2 = new Color(0f, 1f, 0f, 0.8f);

                UICustomColorRegistry.SetColor(testId1, color1);
                UICustomColorRegistry.SetColor(testId2, color2);

                if (!UICustomColorRegistry.TryGetColor(testId1, out var retrieved1))
                    throw new System.Exception("Failed to get color1 before reload");
                if (retrieved1 != color1)
                    throw new System.Exception($"Color mismatch before reload: expected {color1}, got {retrieved1}");

                // Simulate domain reload by reloading from SessionState
                UICustomColorRegistry.Reload();

                if (!UICustomColorRegistry.TryGetColor(testId1, out var reloaded1))
                    throw new System.Exception("Failed to get color1 after reload");
                if (Mathf.Abs(reloaded1.r - color1.r) > 0.001f ||
                    Mathf.Abs(reloaded1.g - color1.g) > 0.001f ||
                    Mathf.Abs(reloaded1.b - color1.b) > 0.001f ||
                    Mathf.Abs(reloaded1.a - color1.a) > 0.001f)
                    throw new System.Exception($"Color mismatch after reload: expected {color1}, got {reloaded1}");

                if (!UICustomColorRegistry.TryGetColor(testId2, out var reloaded2))
                    throw new System.Exception("Failed to get color2 after reload");

                // Test RemoveColor
                UICustomColorRegistry.RemoveColor(testId1);
                UICustomColorRegistry.Reload();

                if (UICustomColorRegistry.TryGetColor(testId1, out _))
                    throw new System.Exception("color1 should have been removed");
                if (!UICustomColorRegistry.TryGetColor(testId2, out _))
                    throw new System.Exception("color2 should still exist");

                // Test ClearAll
                UICustomColorRegistry.ClearAll();
                UICustomColorRegistry.Reload();

                if (UICustomColorRegistry.TryGetColor(testId2, out _))
                    throw new System.Exception("color2 should have been cleared");
            }
            finally
            {
                UICustomColorRegistry.ClearAll();
            }
        }

        static void Test_UIElementEntry_CustomColor()
        {
            UICustomColorRegistry.ClearAll();
            var canvasGo = new GameObject("Canvas", typeof(Canvas));
            try
            {
                var imgGo1 = new GameObject("Image1", typeof(RectTransform), typeof(Image));
                imgGo1.transform.SetParent(canvasGo.transform, false);

                var imgGo2 = new GameObject("Image2", typeof(RectTransform), typeof(Image));
                imgGo2.transform.SetParent(canvasGo.transform, false);
                int id1 = imgGo1.GetHashCode();
                int id2 = imgGo2.GetHashCode();
                var customColor1 = new Color(0.2f, 0.4f, 0.6f, 1f);

                UICustomColorRegistry.SetColor(id1, customColor1);

                var entries = UIRenderTreeCollector.Collect(new[] { canvasGo.GetComponent<Canvas>() });

                if (entries.Count != 2)
                    throw new System.Exception($"Expected 2 entries, got {entries.Count}");

                if (entries[0].InstanceId != id1)
                    throw new System.Exception($"Expected entry 0 InstanceId to be {id1}, got {entries[0].InstanceId}");
                if (!entries[0].CustomColor.HasValue)
                    throw new System.Exception("Expected entry 0 CustomColor to have value");
                if (entries[0].CustomColor.Value != customColor1)
                    throw new System.Exception($"Expected entry 0 CustomColor {customColor1}, got {entries[0].CustomColor.Value}");

                if (entries[1].InstanceId != id2)
                    throw new System.Exception($"Expected entry 1 InstanceId to be {id2}, got {entries[1].InstanceId}");
                if (entries[1].CustomColor.HasValue)
                    throw new System.Exception("Expected entry 1 CustomColor to be null");
            }
            finally
            {
                UICustomColorRegistry.ClearAll();
                Object.DestroyImmediate(canvasGo);
            }
        }

        static void Test_ViewportDepthPlacement()
        {
            var viewport = new UIPreview3DViewport();
            try
            {
                viewport.Initialize();
                viewport.SetExplosionFactor(5f);

                var entry0 = new UIElementEntry { GlobalDrawIndex = 0, WorldRect = new Rect(0, 0, 100, 100) };
                var entry1 = new UIElementEntry { GlobalDrawIndex = 1, WorldRect = new Rect(0, 0, 100, 100) };

                var entries = new List<UIElementEntry> { entry0, entry1 };
                viewport.RebuildFromEntries(entries);

                var field = typeof(UIPreview3DViewport).GetField("_previewObjects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var previewObjects = (List<GameObject>)field.GetValue(viewport);

                if (previewObjects == null || previewObjects.Count != 2)
                    throw new System.Exception($"Expected 2 preview objects, got {previewObjects?.Count ?? 0}");

                float z0 = previewObjects[0].transform.localPosition.z;
                float z1 = previewObjects[1].transform.localPosition.z;

                if (z1 >= z0)
                    throw new System.Exception($"Expected z1 ({z1}) < z0 ({z0}) for negative Z stack direction");
            }
            finally
            {
                viewport.Dispose();
            }
        }

        static void Test_ViewportRenderQueueOrdering()
        {
            var viewport = new UIPreview3DViewport();
            try
            {
                viewport.Initialize();
                viewport.SetExplosionFactor(0f);

                var entry0 = new UIElementEntry { GlobalDrawIndex = 0, WorldRect = new Rect(0, 0, 100, 100) };
                var entry1 = new UIElementEntry { GlobalDrawIndex = 1, WorldRect = new Rect(0, 0, 100, 100) };
                var entry2 = new UIElementEntry { GlobalDrawIndex = 2, WorldRect = new Rect(0, 0, 100, 100) };

                var entries = new List<UIElementEntry> { entry0, entry1, entry2 };
                viewport.RebuildFromEntries(entries);

                var field = typeof(UIPreview3DViewport).GetField("_previewObjects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var previewObjects = (List<GameObject>)field.GetValue(viewport);

                if (previewObjects == null || previewObjects.Count != 3)
                    throw new System.Exception($"Expected 3 preview objects, got {previewObjects?.Count ?? 0}");

                var mr0 = previewObjects[0].GetComponent<MeshRenderer>();
                var mr1 = previewObjects[1].GetComponent<MeshRenderer>();
                var mr2 = previewObjects[2].GetComponent<MeshRenderer>();

                if (mr0.sharedMaterial == null || mr1.sharedMaterial == null || mr2.sharedMaterial == null)
                    throw new System.Exception("MeshRenderers must have non-null sharedMaterial");

                if (mr0.sharedMaterial.renderQueue != 3000)
                    throw new System.Exception($"Expected mr0 renderQueue 3000, got {mr0.sharedMaterial.renderQueue}");
                if (mr1.sharedMaterial.renderQueue != 3001)
                    throw new System.Exception($"Expected mr1 renderQueue 3001, got {mr1.sharedMaterial.renderQueue}");
                if (mr2.sharedMaterial.renderQueue != 3002)
                    throw new System.Exception($"Expected mr2 renderQueue 3002, got {mr2.sharedMaterial.renderQueue}");
            }
            finally
            {
                viewport.Dispose();
            }
        }

        static void Test_SpatialOcclusion()
        {
            var canvasGo = new GameObject("RootCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            var overlayCanvasGo = new GameObject("OverlayCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            try
            {
                var rootCanvas = canvasGo.GetComponent<Canvas>();
                rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                rootCanvas.sortingOrder = 0;

                var btnGo = new GameObject("TestButton", typeof(RectTransform), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(canvasGo.transform, false);
                var btnRect = btnGo.GetComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(100, 50);
                btnRect.position = Vector3.zero;

                var overlayCanvas = overlayCanvasGo.GetComponent<Canvas>();
                overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                overlayCanvas.overrideSorting = true;
                overlayCanvas.sortingOrder = 10;

                var blockerGo = new GameObject("OverlayImage", typeof(RectTransform), typeof(Image));
                blockerGo.transform.SetParent(overlayCanvasGo.transform, false);
                var blockerRect = blockerGo.GetComponent<RectTransform>();
                blockerRect.sizeDelta = new Vector2(100, 50);
                blockerRect.position = Vector3.zero;
                var blockerImg = blockerGo.GetComponent<Image>();
                blockerImg.color = Color.white;
                blockerImg.raycastTarget = true;

                var entries = UIRenderTreeCollector.Collect(new Canvas[] { rootCanvas, overlayCanvas });
                UIDiagnosticAnalyzer.Analyze(entries);

                if (entries.Count != 2)
                    throw new System.Exception($"Expected 2 entries, got {entries.Count}");

                var lower = entries[0];
                var upper = entries[1];

                if ((upper.Flags & DiagnosticFlags.OcclusionBlocker) == 0)
                    throw new System.Exception("Expected upper element to have OcclusionBlocker flag");
                if (upper.OcclusionPairs == null || upper.OcclusionPairs.Count != 1)
                    throw new System.Exception("Expected upper element to have 1 occlusion pair");
                if (upper.OcclusionPairs[0].TargetName != "TestButton")
                    throw new System.Exception($"Expected target name 'TestButton', got '{upper.OcclusionPairs[0].TargetName}'");
                if (Mathf.Abs(upper.OcclusionPairs[0].OverlapPercentage - 100f) > 0.01f)
                    throw new System.Exception($"Expected 100% overlap, got {upper.OcclusionPairs[0].OverlapPercentage}");

                if (lower.OcclusionPairs == null || lower.OcclusionPairs.Count != 1)
                    throw new System.Exception("Expected lower element to have 1 occlusion pair");
            }
            finally
            {
                Object.DestroyImmediate(overlayCanvasGo);
                Object.DestroyImmediate(canvasGo);
            }
        }

        static void Test_NestedLabelRaycast()
        {
            var canvasGo = new GameObject("RootCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            try
            {
                var btnGo = new GameObject("SubmitButton", typeof(RectTransform), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(canvasGo.transform, false);
                var btnImg = btnGo.GetComponent<Image>();
                btnImg.raycastTarget = true;

                var labelGo = new GameObject("ButtonLabel", typeof(RectTransform), typeof(Text));
                labelGo.transform.SetParent(btnGo.transform, false);
                var labelText = labelGo.GetComponent<Text>();
                labelText.raycastTarget = true;

                var entries = UIRenderTreeCollector.Collect(new Canvas[] { canvasGo.GetComponent<Canvas>() });
                UIDiagnosticAnalyzer.Analyze(entries);

                if (entries.Count != 2)
                    throw new System.Exception($"Expected 2 entries, got {entries.Count}");

                var btn = entries[0];
                var label = entries[1];

                if ((btn.Flags & DiagnosticFlags.NestedLabelRaycast) != 0)
                    throw new System.Exception("Button root should not have NestedLabelRaycast flag");
                if ((label.Flags & DiagnosticFlags.NestedLabelRaycast) == 0)
                    throw new System.Exception("Child label should have NestedLabelRaycast flag");
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        static void Test_ExporterMarkdownAnomalies()
        {
            var canvasGo = new GameObject("RootCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            var overlayCanvasGo = new GameObject("OverlayCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            try
            {
                var btnGo = new GameObject("TestButton", typeof(RectTransform), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(canvasGo.transform, false);
                var btnRect = btnGo.GetComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(100, 50);
                btnRect.position = Vector3.zero;

                var overlayCanvas = overlayCanvasGo.GetComponent<Canvas>();
                overlayCanvas.overrideSorting = true;
                overlayCanvas.sortingOrder = 10;

                var blockerGo = new GameObject("OverlayImage", typeof(RectTransform), typeof(Image));
                blockerGo.transform.SetParent(overlayCanvasGo.transform, false);
                var blockerRect = blockerGo.GetComponent<RectTransform>();
                blockerRect.sizeDelta = new Vector2(100, 50);
                blockerRect.position = Vector3.zero;
                var blockerImg = blockerGo.GetComponent<Image>();
                blockerImg.raycastTarget = true;

                var healthyGo = new GameObject("HealthyBg", typeof(RectTransform), typeof(Image));
                healthyGo.transform.SetParent(canvasGo.transform, false);
                var healthyImg = healthyGo.GetComponent<Image>();
                healthyImg.raycastTarget = false;

                var entries = UIRenderTreeCollector.Collect(new Canvas[] { canvasGo.GetComponent<Canvas>(), overlayCanvas });
                UIDiagnosticAnalyzer.Analyze(entries);

                string md = UIAIContextExporter.ExportToCompactMarkdown(entries, canvasGo.GetComponent<Canvas>(), ExportMode.Anomalies);

                if (!md.Contains("[SPATIAL BLOCK]"))
                    throw new System.Exception("Expected markdown to contain [SPATIAL BLOCK]");
                if (!md.Contains("'OverlayImage' intercepts 100% of Button 'TestButton'"))
                    throw new System.Exception("Expected markdown to contain spatial block triplet");
                if (!md.Contains("Fix: Disable Raycast Target or lower Canvas sorting order."))
                    throw new System.Exception("Expected markdown to contain spatial block fix guidance");
                if (md.Contains("HealthyBg"))
                    throw new System.Exception("Expected markdown in Anomalies mode to omit HealthyBg");
            }
            finally
            {
                Object.DestroyImmediate(overlayCanvasGo);
                Object.DestroyImmediate(canvasGo);
            }
        }

        static void Test_ExporterJsonAnomalies()
        {
            var canvasGo = new GameObject("RootCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            var overlayCanvasGo = new GameObject("OverlayCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            try
            {
                var btnGo = new GameObject("TestButton", typeof(RectTransform), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(canvasGo.transform, false);
                var btnRect = btnGo.GetComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(100, 50);
                btnRect.position = Vector3.zero;

                var overlayCanvas = overlayCanvasGo.GetComponent<Canvas>();
                overlayCanvas.overrideSorting = true;
                overlayCanvas.sortingOrder = 10;

                var blockerGo = new GameObject("OverlayImage", typeof(RectTransform), typeof(Image));
                blockerGo.transform.SetParent(overlayCanvasGo.transform, false);
                var blockerRect = blockerGo.GetComponent<RectTransform>();
                blockerRect.sizeDelta = new Vector2(100, 50);
                blockerRect.position = Vector3.zero;
                var blockerImg = blockerGo.GetComponent<Image>();
                blockerImg.raycastTarget = true;

                var healthyGo = new GameObject("HealthyBg", typeof(RectTransform), typeof(Image));
                healthyGo.transform.SetParent(canvasGo.transform, false);
                var healthyImg = healthyGo.GetComponent<Image>();
                healthyImg.raycastTarget = false;

                var entries = UIRenderTreeCollector.Collect(new Canvas[] { canvasGo.GetComponent<Canvas>(), overlayCanvas });
                UIDiagnosticAnalyzer.Analyze(entries);

                string json = UIAIContextExporter.ExportToJson(entries, canvasGo.GetComponent<Canvas>(), ExportMode.Anomalies, prettyPrint: true);

                if (!json.Contains("\"flag\": \"SPATIAL_BLOCK\""))
                    throw new System.Exception("Expected json to contain SPATIAL_BLOCK flag");
                if (!json.Contains("\"fixAction\":"))
                    throw new System.Exception("Expected json to contain fixAction object");
                if (!json.Contains("\"component\": \"Image\""))
                    throw new System.Exception("Expected fixAction component to be Image");
                if (!json.Contains("\"property\": \"raycastTarget\""))
                    throw new System.Exception("Expected fixAction property to be raycastTarget");
                if (!json.Contains("\"value\": false"))
                    throw new System.Exception("Expected fixAction value to be false");
                if (json.Contains("\"name\": \"HealthyBg\""))
                    throw new System.Exception("Expected json elements in Anomalies mode to omit HealthyBg");
            }
            finally
            {
                Object.DestroyImmediate(overlayCanvasGo);
                Object.DestroyImmediate(canvasGo);
            }
        }

        static void Test_CLI_ParseExportMode()
        {
            if (UIDepthInspectorCLI.ParseExportMode("anomalies") != ExportMode.Anomalies)
                throw new System.Exception("Expected 'anomalies' to parse to ExportMode.Anomalies");
            if (UIDepthInspectorCLI.ParseExportMode("compact") != ExportMode.Compact)
                throw new System.Exception("Expected 'compact' to parse to ExportMode.Compact");
            if (UIDepthInspectorCLI.ParseExportMode("full") != ExportMode.Full)
                throw new System.Exception("Expected 'full' to parse to ExportMode.Full");
            if (UIDepthInspectorCLI.ParseExportMode("") != ExportMode.Anomalies)
                throw new System.Exception("Expected empty string to default to ExportMode.Anomalies");
            if (UIDepthInspectorCLI.ParseExportMode("invalid") != ExportMode.Anomalies)
                throw new System.Exception("Expected invalid string to default to ExportMode.Anomalies");
        }

        static void Test_AutoExporter()
        {
            var canvasGo = new GameObject("AutoExportCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            try
            {
                var childGo = new GameObject("GhostImage", typeof(RectTransform), typeof(Image));
                childGo.transform.SetParent(canvasGo.transform, false);
                var img = childGo.GetComponent<Image>();
                img.color = new Color(1, 1, 1, 0); // alpha 0 ghost blocker
                img.raycastTarget = true;

                UIAIContextAutoExporter.ExportActiveContext();

                if (!File.Exists(UIAIContextAutoExporter.ActiveContextMarkdown))
                    throw new System.Exception("Expected ActiveContextMarkdown file to exist");
                if (!File.Exists(UIAIContextAutoExporter.ActiveContextJson))
                    throw new System.Exception("Expected ActiveContextJson file to exist");

                string md = File.ReadAllText(UIAIContextAutoExporter.ActiveContextMarkdown);
                string json = File.ReadAllText(UIAIContextAutoExporter.ActiveContextJson);

                if (!md.Contains("AutoExportCanvas") || !md.Contains("GhostImage"))
                    throw new System.Exception("Expected markdown to contain canvas and element names");
                if (!json.Contains("GhostImage") || !json.Contains("GHOST_BLOCKER"))
                    throw new System.Exception("Expected json to contain GhostImage and GHOST_BLOCKER");
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }
    }
}
