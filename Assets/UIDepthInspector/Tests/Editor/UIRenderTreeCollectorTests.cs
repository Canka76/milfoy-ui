using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UIDepthInspector.Editor.Core;
using UIDepthInspector.Editor.Diagnostics;

namespace UIDepthInspector.Editor.Tests
{
    public class UIRenderTreeCollectorTests
    {
        List<GameObject> _createdObjects;

        [SetUp]
        public void SetUp()
        {
            _createdObjects = new List<GameObject>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _createdObjects)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            _createdObjects.Clear();
        }

        GameObject CreateGameObject(string name, params System.Type[] components)
        {
            var go = new GameObject(name, components);
            _createdObjects.Add(go);
            return go;
        }

        [Test]
        public void Collect_EmptyScene_ReturnsEmptyList()
        {
            var results = UIRenderTreeCollector.Collect();
            Assert.IsNotNull(results);
        }

        [Test]
        public void Collect_SingleCanvasWithImages_ReturnsCorrectDrawOrder()
        {
            // Root Canvas
            var canvasGo = CreateGameObject("Canvas", typeof(Canvas), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Background (sibling 0)
            var bgGo = CreateGameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(canvasGo.transform, false);

            // Button (sibling 1)
            var btnGo = CreateGameObject("Button", typeof(RectTransform), typeof(Image));
            btnGo.transform.SetParent(canvasGo.transform, false);

            // Label inside Button (child of sibling 1)
            var lblGo = CreateGameObject("Label", typeof(RectTransform), typeof(Text));
            lblGo.transform.SetParent(btnGo.transform, false);

            var results = UIRenderTreeCollector.Collect(new[] { canvas });

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("Background", results[0].Name);
            Assert.AreEqual(0, results[0].GlobalDrawIndex);

            Assert.AreEqual("Button", results[1].Name);
            Assert.AreEqual(1, results[1].GlobalDrawIndex);

            Assert.AreEqual("Label", results[2].Name);
            Assert.AreEqual(2, results[2].GlobalDrawIndex);
        }

        [Test]
        public void Collect_MultipleRootCanvases_SortsBySortingOrderAndLayer()
        {
            // Canvas A (sorting order 10)
            var canvasAGo = CreateGameObject("CanvasA", typeof(Canvas));
            var canvasA = canvasAGo.GetComponent<Canvas>();
            canvasA.sortingOrder = 10;
            var imgA = CreateGameObject("ImageA", typeof(RectTransform), typeof(Image));
            imgA.transform.SetParent(canvasAGo.transform, false);

            // Canvas B (sorting order 0 - should render first)
            var canvasBGo = CreateGameObject("CanvasB", typeof(Canvas));
            var canvasB = canvasBGo.GetComponent<Canvas>();
            canvasB.sortingOrder = 0;
            var imgB = CreateGameObject("ImageB", typeof(RectTransform), typeof(Image));
            imgB.transform.SetParent(canvasBGo.transform, false);

            var results = UIRenderTreeCollector.Collect(new[] { canvasA, canvasB });

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("ImageB", results[0].Name);
            Assert.AreEqual("ImageA", results[1].Name);
        }

        [Test]
        public void Collect_CanvasGroup_CalculatesCumulativeAlphaAndRaycastBlocking()
        {
            var canvasGo = CreateGameObject("Canvas", typeof(Canvas));
            var canvas = canvasGo.GetComponent<Canvas>();

            var panelGo = CreateGameObject("Panel", typeof(RectTransform), typeof(CanvasGroup));
            panelGo.transform.SetParent(canvasGo.transform, false);
            var cg1 = panelGo.GetComponent<CanvasGroup>();
            cg1.alpha = 0.5f;
            cg1.blocksRaycasts = true;

            var subPanelGo = CreateGameObject("SubPanel", typeof(RectTransform), typeof(CanvasGroup));
            subPanelGo.transform.SetParent(panelGo.transform, false);
            var cg2 = subPanelGo.GetComponent<CanvasGroup>();
            cg2.alpha = 0.5f;
            cg2.blocksRaycasts = false;

            var imgGo = CreateGameObject("Image", typeof(RectTransform), typeof(Image));
            imgGo.transform.SetParent(subPanelGo.transform, false);
            var img = imgGo.GetComponent<Image>();
            img.color = new Color(1, 1, 1, 1);

            var results = UIRenderTreeCollector.Collect(new[] { canvas });

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(0.25f, results[0].EffectiveAlpha, 0.001f);
            Assert.IsFalse(results[0].BlocksRaycasts);
        }
    }

    public class UIDiagnosticAnalyzerTests
    {
        List<GameObject> _createdObjects;

        [SetUp]
        public void SetUp()
        {
            _createdObjects = new List<GameObject>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _createdObjects)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            _createdObjects.Clear();
        }

        [Test]
        public void Analyze_GhostBlockerWithZeroAlpha_SetsGhostBlockerFlag()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas));
            _createdObjects.Add(canvasGo);
            var canvas = canvasGo.GetComponent<Canvas>();

            var imgGo = new GameObject("GhostImage", typeof(RectTransform), typeof(Image));
            _createdObjects.Add(imgGo);
            imgGo.transform.SetParent(canvasGo.transform, false);

            var img = imgGo.GetComponent<Image>();
            img.color = new Color(1, 1, 1, 0); // alpha = 0
            img.raycastTarget = true;

            var entries = UIRenderTreeCollector.Collect(new[] { canvas });
            UIDiagnosticAnalyzer.Analyze(entries);

            Assert.AreEqual(1, entries.Count);
            Assert.IsTrue((entries[0].Flags & DiagnosticFlags.GhostBlocker) != 0, "Expected GhostBlocker flag for alpha=0 with raycastTarget=true");
        }

        [Test]
        public void Analyze_PassiveVisual_SetsPassiveVisualFlag()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas));
            _createdObjects.Add(canvasGo);
            var canvas = canvasGo.GetComponent<Canvas>();

            var imgGo = new GameObject("PassiveImage", typeof(RectTransform), typeof(Image));
            _createdObjects.Add(imgGo);
            imgGo.transform.SetParent(canvasGo.transform, false);

            var img = imgGo.GetComponent<Image>();
            img.color = new Color(1, 1, 1, 1);
            img.raycastTarget = false;

            var entries = UIRenderTreeCollector.Collect(new[] { canvas });
            UIDiagnosticAnalyzer.Analyze(entries);

            Assert.AreEqual(1, entries.Count);
            Assert.IsTrue((entries[0].Flags & DiagnosticFlags.PassiveVisual) != 0);
            Assert.IsFalse((entries[0].Flags & DiagnosticFlags.RaycastBlocker) != 0);
        }

        [Test]
        public void Analyze_MaskAndRectMask2D_SetsCorrespondingMaskFlags()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas));
            _createdObjects.Add(canvasGo);
            var canvas = canvasGo.GetComponent<Canvas>();

            var maskGo = new GameObject("MaskObject", typeof(RectTransform), typeof(Image), typeof(Mask));
            _createdObjects.Add(maskGo);
            maskGo.transform.SetParent(canvasGo.transform, false);

            var rectMaskGo = new GameObject("RectMaskObject", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            _createdObjects.Add(rectMaskGo);
            rectMaskGo.transform.SetParent(canvasGo.transform, false);

            var entries = UIRenderTreeCollector.Collect(new[] { canvas });
            UIDiagnosticAnalyzer.Analyze(entries);

            Assert.AreEqual(2, entries.Count);
            Assert.IsTrue((entries[0].Flags & DiagnosticFlags.HasMask) != 0);
            Assert.IsTrue((entries[1].Flags & DiagnosticFlags.HasRectMask2D) != 0);
        }
    }
}
