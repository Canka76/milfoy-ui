using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UIDepthInspector.Editor.Core;
using UIDepthInspector.Editor.Diagnostics;
using UIDepthInspector.Editor.Export;

namespace UIDepthInspector.Tests.Editor
{
    [TestFixture]
    public class UIAIContextExporterTests
    {
        private GameObject _rootCanvasGo;
        private Canvas _canvas;

        [SetUp]
        public void SetUp()
        {
            _rootCanvasGo = new GameObject("TestCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            _canvas = _rootCanvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        [TearDown]
        public void TearDown()
        {
            if (_rootCanvasGo != null)
                Object.DestroyImmediate(_rootCanvasGo);
        }

        [Test]
        public void ExportToCompactMarkdown_WithAnomalies_GeneratesExpectedStructure()
        {
            // Arrange
            var childGo = new GameObject("GhostImage", typeof(RectTransform), typeof(Image));
            childGo.transform.SetParent(_rootCanvasGo.transform, false);
            var img = childGo.GetComponent<Image>();
            img.color = new Color(1, 1, 1, 0); // alpha 0 ghost blocker
            var entries = UIRenderTreeCollector.Collect(new Canvas[] { _canvas });
            UIDiagnosticAnalyzer.Analyze(entries);

            // Act
            string markdown = UIAIContextExporter.ExportToCompactMarkdown(entries, _canvas);

            // Assert
            Assert.IsNotNull(markdown);
            Assert.IsTrue(markdown.Contains("AI Diagnostic Report"), "Should contain header");
            Assert.IsTrue(markdown.Contains("TestCanvas"), "Should contain canvas name");
            Assert.IsTrue(markdown.Contains("GhostImage"), "Should list element name");
            Assert.IsTrue(markdown.Contains("GHOST_BLOCKER") || markdown.Contains("GhostBlocker"), "Should flag ghost blocker");
            Assert.IsTrue(markdown.Contains("Remediation") || markdown.Contains("Fix:"), "Should contain actionable fix guidance");
        }

        [Test]
        public void ExportToCompactMarkdown_AnomaliesMode_OutputsOnlyAnomalousElements()
        {
            // Healthy element
            var healthyGo = new GameObject("HealthyImage", typeof(RectTransform), typeof(Image));
            healthyGo.transform.SetParent(_rootCanvasGo.transform, false);
            var healthyImg = healthyGo.GetComponent<Image>();
            healthyImg.color = Color.white;
            healthyImg.raycastTarget = false;

            // Anomalous element
            var ghostGo = new GameObject("GhostElement", typeof(RectTransform), typeof(Image));
            ghostGo.transform.SetParent(_rootCanvasGo.transform, false);
            var ghostImg = ghostGo.GetComponent<Image>();
            ghostImg.color = new Color(1, 1, 1, 0);
            ghostImg.raycastTarget = true;

            var entries = UIRenderTreeCollector.Collect(new Canvas[] { _canvas });
            UIDiagnosticAnalyzer.Analyze(entries);

            string markdown = UIAIContextExporter.ExportToCompactMarkdown(entries, _canvas, ExportMode.Anomalies);

            Assert.IsNotNull(markdown);
            Assert.IsTrue(markdown.Contains("GhostElement"), "Should contain anomalous element");
            Assert.IsFalse(markdown.Contains("HealthyImage"), "Should omit healthy element in anomalies mode");
            Assert.IsTrue(markdown.Contains("GHOST_BLOCKER"), "Should list GHOST_BLOCKER anomaly");
        }

        [Test]
        public void ExportToCompactMarkdown_SpatialOcclusion_OutputsSpatialBlockTriplet()
        {
            var btnGo = new GameObject("TestButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(_rootCanvasGo.transform, false);
            var btnRect = btnGo.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(100, 50);
            btnRect.position = Vector3.zero;

            var overlayCanvasGo = new GameObject("OverlayCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
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

            try
            {
                var entries = UIRenderTreeCollector.Collect(new Canvas[] { _canvas, overlayCanvas });
                UIDiagnosticAnalyzer.Analyze(entries);

                string markdown = UIAIContextExporter.ExportToCompactMarkdown(entries, _canvas, ExportMode.Anomalies);

                Assert.IsNotNull(markdown);
                Assert.IsTrue(markdown.Contains("[SPATIAL BLOCK]"), "Should contain [SPATIAL BLOCK] tag");
                Assert.IsTrue(markdown.Contains("'OverlayImage' intercepts 100% of Button 'TestButton'"), "Should contain exact blocker/target triplet");
                Assert.IsTrue(markdown.Contains("Fix: Disable Raycast Target or lower Canvas sorting order."), "Should contain fix guidance");
            }
            finally
            {
                Object.DestroyImmediate(overlayCanvasGo);
            }
        }

        [Test]
        public void ExportToJson_GeneratesValidJsonWithElementsAndAnomalies()
        {
            // Arrange
            var childGo = new GameObject("BtnText", typeof(RectTransform), typeof(Image));
            childGo.transform.SetParent(_rootCanvasGo.transform, false);

            var entries = UIRenderTreeCollector.Collect(new Canvas[] { _canvas });
            UIDiagnosticAnalyzer.Analyze(entries);
            string json = UIAIContextExporter.ExportToJson(entries, _canvas, prettyPrint: true);

            // Assert
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("\"canvasName\": \"TestCanvas\""), "Should have canvasName in json");
            Assert.IsTrue(json.Contains("\"elements\":"), "Should contain elements list");
            Assert.IsTrue(json.Contains("\"anomalies\":"), "Should contain anomalies list");
        }

        [Test]
        public void ExportToJson_AnomaliesMode_FiltersElementsAndIncludesFixAction()
        {
            // Healthy element
            var healthyGo = new GameObject("HealthyElement", typeof(RectTransform), typeof(Image));
            healthyGo.transform.SetParent(_rootCanvasGo.transform, false);
            var healthyImg = healthyGo.GetComponent<Image>();
            healthyImg.color = Color.white;
            healthyImg.raycastTarget = false;

            // Ghost blocker
            var ghostGo = new GameObject("GhostElement", typeof(RectTransform), typeof(Image));
            ghostGo.transform.SetParent(_rootCanvasGo.transform, false);
            var ghostImg = ghostGo.GetComponent<Image>();
            ghostImg.color = new Color(1, 1, 1, 0);
            ghostImg.raycastTarget = true;

            var entries = UIRenderTreeCollector.Collect(new Canvas[] { _canvas });
            UIDiagnosticAnalyzer.Analyze(entries);

            string json = UIAIContextExporter.ExportToJson(entries, _canvas, ExportMode.Anomalies, prettyPrint: true);

            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("\"flag\": \"GHOST_BLOCKER\""), "Should contain ghost blocker anomaly");
            Assert.IsTrue(json.Contains("\"fixAction\":"), "Should contain fixAction payload");
            Assert.IsTrue(json.Contains("\"property\": \"raycastTarget\""), "Should target raycastTarget property");
            Assert.IsTrue(json.Contains("\"value\": false"), "Should set value to false");
            Assert.IsFalse(json.Contains("\"name\": \"HealthyElement\""), "Should not include healthy elements in elements list in Anomalies mode");
        }

        [Test]
        public void ExportToJson_SpatialOcclusion_IncludesStructuredFixAction()
        {
            var btnGo = new GameObject("TargetButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(_rootCanvasGo.transform, false);
            var btnRect = btnGo.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(100, 50);
            btnRect.position = Vector3.zero;

            var overlayCanvasGo = new GameObject("OverlayCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            var overlayCanvas = overlayCanvasGo.GetComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 5;

            var blockerGo = new GameObject("BlockerImage", typeof(RectTransform), typeof(Image));
            blockerGo.transform.SetParent(overlayCanvasGo.transform, false);
            var blockerRect = blockerGo.GetComponent<RectTransform>();
            blockerRect.sizeDelta = new Vector2(100, 50);
            blockerRect.position = Vector3.zero;
            var blockerImg = blockerGo.GetComponent<Image>();
            blockerImg.color = Color.white;
            blockerImg.raycastTarget = true;

            try
            {
                var entries = UIRenderTreeCollector.Collect(new Canvas[] { _canvas, overlayCanvas });
                UIDiagnosticAnalyzer.Analyze(entries);

                string json = UIAIContextExporter.ExportToJson(entries, _canvas, ExportMode.Anomalies, prettyPrint: true);

                Assert.IsNotNull(json);
                Assert.IsTrue(json.Contains("\"flag\": \"SPATIAL_BLOCK\""), "Should contain SPATIAL_BLOCK flag");
                Assert.IsTrue(json.Contains("\"component\": \"Image\""), "Should identify component as Image");
                Assert.IsTrue(json.Contains("\"property\": \"raycastTarget\""), "Should identify property as raycastTarget");
                Assert.IsTrue(json.Contains("\"value\": false"), "Should set value to false");
                Assert.IsTrue(json.Contains("BlockerImage"), "Should identify blocker target path");
            }
            finally
            {
                Object.DestroyImmediate(overlayCanvasGo);
            }
        }

        [Test]
        public void UIDepthInspectorCLI_GetArg_ParsesParametersCorrectly()
        {
            string[] args = new string[] { "-batchmode", "-dumpFormat", "json", "-dumpMode", "compact", "-dumpOutput", "Temp/ui.json" };

            string format = UIDepthInspectorCLI.GetArg(args, "-dumpFormat", "md");
            string mode = UIDepthInspectorCLI.GetArg(args, "-dumpMode", "anomalies");
            string output = UIDepthInspectorCLI.GetArg(args, "-dumpOutput", "");
            string missing = UIDepthInspectorCLI.GetArg(args, "-missingParam", "defaultVal");

            Assert.AreEqual("json", format);
            Assert.AreEqual("compact", mode);
            Assert.AreEqual("Temp/ui.json", output);
            Assert.AreEqual("defaultVal", missing);
        }

        [Test]
        public void UIDepthInspectorCLI_ParseExportMode_ReturnsExpectedModes()
        {
            Assert.AreEqual(ExportMode.Anomalies, UIDepthInspectorCLI.ParseExportMode("anomalies"));
            Assert.AreEqual(ExportMode.Anomalies, UIDepthInspectorCLI.ParseExportMode("ANOMALIES"));
            Assert.AreEqual(ExportMode.Compact, UIDepthInspectorCLI.ParseExportMode("compact"));
            Assert.AreEqual(ExportMode.Compact, UIDepthInspectorCLI.ParseExportMode("COMPACT"));
            Assert.AreEqual(ExportMode.Full, UIDepthInspectorCLI.ParseExportMode("full"));
            Assert.AreEqual(ExportMode.Full, UIDepthInspectorCLI.ParseExportMode("FULL"));
            Assert.AreEqual(ExportMode.Anomalies, UIDepthInspectorCLI.ParseExportMode(null));
            Assert.AreEqual(ExportMode.Anomalies, UIDepthInspectorCLI.ParseExportMode("unknown_mode"));
        }

        [Test]
        public void Analyze_SpatialOcclusion_HigherSortingOrderCanvasBlocksLowerButton_SetsOcclusionBlockerAndPopulatesPairs()
        {
            // Arrange: Canvas 1 with lower Button
            var btnGo = new GameObject("TestButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(_rootCanvasGo.transform, false);
            var btnRect = btnGo.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(100, 50);
            btnRect.position = Vector3.zero;

            // Canvas 2 with higher sorting order and an overlay Image
            var overlayCanvasGo = new GameObject("OverlayCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
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

            try
            {
                var entries = UIRenderTreeCollector.Collect(new Canvas[] { _canvas, overlayCanvas });
                UIDiagnosticAnalyzer.Analyze(entries);

                Assert.AreEqual(2, entries.Count);

                var lowerEntry = entries[0];
                var upperEntry = entries[1];

                // Upper element must be flagged as OcclusionBlocker
                Assert.IsTrue((upperEntry.Flags & DiagnosticFlags.OcclusionBlocker) != 0, "Upper element should have OcclusionBlocker flag");
                Assert.IsNotNull(upperEntry.OcclusionPairs, "Upper element OcclusionPairs should be populated");
                Assert.AreEqual(1, upperEntry.OcclusionPairs.Count);
                Assert.AreEqual("OverlayImage", upperEntry.OcclusionPairs[0].BlockerName);
                Assert.AreEqual("TestButton", upperEntry.OcclusionPairs[0].TargetName);
                Assert.AreEqual(100f, upperEntry.OcclusionPairs[0].OverlapPercentage, 0.01f);

                // Lower element must also have OcclusionPairs recorded
                Assert.IsNotNull(lowerEntry.OcclusionPairs, "Lower element OcclusionPairs should be populated");
                Assert.AreEqual(1, lowerEntry.OcclusionPairs.Count);
                Assert.AreEqual("OverlayImage", lowerEntry.OcclusionPairs[0].BlockerName);
                Assert.AreEqual("TestButton", lowerEntry.OcclusionPairs[0].TargetName);
            }
            finally
            {
                Object.DestroyImmediate(overlayCanvasGo);
            }
        }

        [Test]
        public void Analyze_NestedLabelRaycast_ChildTextUnderButtonWithRaycastTarget_SetsNestedLabelRaycastFlag()
        {
            // Button root
            var btnGo = new GameObject("SubmitButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(_rootCanvasGo.transform, false);
            var btnImg = btnGo.GetComponent<Image>();
            btnImg.raycastTarget = true;

            // Child label with raycastTarget = true (anti-pattern)
            var labelGo = new GameObject("ButtonLabel", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(btnGo.transform, false);
            var labelText = labelGo.GetComponent<Text>();
            labelText.raycastTarget = true;

            var entries = UIRenderTreeCollector.Collect(new Canvas[] { _canvas });
            UIDiagnosticAnalyzer.Analyze(entries);

            Assert.AreEqual(2, entries.Count);
            var btnEntry = entries[0];
            var labelEntry = entries[1];

            Assert.IsFalse((btnEntry.Flags & DiagnosticFlags.NestedLabelRaycast) != 0, "Button root should not have NestedLabelRaycast flag");
            Assert.IsTrue((labelEntry.Flags & DiagnosticFlags.NestedLabelRaycast) != 0, "Child label should have NestedLabelRaycast flag");
        }

        [Test]
        public void Analyze_SpatialOcclusion_NonOverlapping_DoesNotSetOcclusionBlocker()
        {
            // Button at (0, 0)
            var btnGo = new GameObject("ButtonA", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(_rootCanvasGo.transform, false);
            var btnRect = btnGo.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(100, 50);
            btnRect.position = new Vector3(0, 0, 0);

            // Sibling image at (500, 500)
            var imgGo = new GameObject("ImageB", typeof(RectTransform), typeof(Image));
            imgGo.transform.SetParent(_rootCanvasGo.transform, false);
            var imgRect = imgGo.GetComponent<RectTransform>();
            imgRect.sizeDelta = new Vector2(100, 50);
            imgRect.position = new Vector3(500, 500, 0);
            var img = imgGo.GetComponent<Image>();
            img.raycastTarget = true;

            var entries = UIRenderTreeCollector.Collect(new Canvas[] { _canvas });
            UIDiagnosticAnalyzer.Analyze(entries);

            Assert.AreEqual(2, entries.Count);
            Assert.IsFalse((entries[1].Flags & DiagnosticFlags.OcclusionBlocker) != 0, "Non-overlapping image should not have OcclusionBlocker flag");
        }

        [Test]
        public void AutoExporter_ExportActiveContext_WritesAnomaliesContextFiles()
        {
            var childGo = new GameObject("GhostImage", typeof(RectTransform), typeof(Image));
            childGo.transform.SetParent(_rootCanvasGo.transform, false);
            var img = childGo.GetComponent<Image>();
            img.color = new Color(1, 1, 1, 0); // alpha 0 ghost blocker
            img.raycastTarget = true;

            UIAIContextAutoExporter.ExportActiveContext();

            Assert.IsTrue(File.Exists(UIAIContextAutoExporter.ActiveContextMarkdown), "ActiveContextMarkdown should exist");
            Assert.IsTrue(File.Exists(UIAIContextAutoExporter.ActiveContextJson), "ActiveContextJson should exist");

            string md = File.ReadAllText(UIAIContextAutoExporter.ActiveContextMarkdown);
            string json = File.ReadAllText(UIAIContextAutoExporter.ActiveContextJson);

            Assert.IsTrue(md.Contains("TestCanvas"));
            Assert.IsTrue(md.Contains("GhostImage"));
            Assert.IsTrue(json.Contains("GhostImage"));
            Assert.IsTrue(json.Contains("GHOST_BLOCKER"));
        }
    }
}
