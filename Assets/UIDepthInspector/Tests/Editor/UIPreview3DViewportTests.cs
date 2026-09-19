using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UIDepthInspector.Editor.Core;
using UIDepthInspector.Editor.Viewport;

namespace UIDepthInspector.Editor.Tests
{
    public class UIPreview3DViewportTests
    {
        UIPreview3DViewport _viewport;

        [SetUp]
        public void SetUp()
        {
            _viewport = new UIPreview3DViewport();
            _viewport.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _viewport?.Dispose();
            _viewport = null;
        }

        [Test]
        public void HigherDrawIndex_IsPlacedCloserToCameraAlongNegativeZ()
        {
            _viewport.SetExplosionFactor(5f);

            var entry0 = new UIElementEntry { GlobalDrawIndex = 0, WorldRect = new Rect(0, 0, 100, 100) };
            var entry1 = new UIElementEntry { GlobalDrawIndex = 1, WorldRect = new Rect(0, 0, 100, 100) };

            var entries = new List<UIElementEntry> { entry0, entry1 };
            _viewport.RebuildFromEntries(entries);

            var field = typeof(UIPreview3DViewport).GetField("_previewObjects", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "Field _previewObjects should exist on UIPreview3DViewport");

            var previewObjects = (List<GameObject>)field.GetValue(_viewport);

            Assert.IsNotNull(previewObjects);
            Assert.AreEqual(2, previewObjects.Count);

            float z0 = previewObjects[0].transform.localPosition.z;
            float z1 = previewObjects[1].transform.localPosition.z;

            // z1 (higher draw order / foreground) should be strictly less than z0 (closer to camera along -Z)
            Assert.Less(z1, z0, "Higher draw order layer must be closer to camera along negative Z");
        }

        [Test]
        public void PortraitAndLandscapeCanvases_AreStrictlyCenteredAtOrigin()
        {
            // Simulate a 1080x1920 mobile portrait canvas
            var rootCanvas = new UIElementEntry { GlobalDrawIndex = 0, WorldRect = new Rect(0, 0, 1080, 1920) };
            var centerElement = new UIElementEntry { GlobalDrawIndex = 1, WorldRect = new Rect(440, 860, 200, 200) }; // Centered on canvas

            var entries = new List<UIElementEntry> { rootCanvas, centerElement };
            _viewport.RebuildFromEntries(entries);

            var field = typeof(UIPreview3DViewport).GetField("_previewObjects", BindingFlags.NonPublic | BindingFlags.Instance);
            var previewObjects = (List<GameObject>)field.GetValue(_viewport);

            Assert.IsNotNull(previewObjects);
            Assert.AreEqual(2, previewObjects.Count);

            // The root canvas center should be exactly at X=0, Y=0 in the 3D preview world
            Vector3 rootPos = previewObjects[0].transform.localPosition;
            Assert.AreEqual(0f, rootPos.x, 0.001f, "Root canvas X position must be centered at 0");
            Assert.AreEqual(0f, rootPos.y, 0.001f, "Root canvas Y position must be centered at 0");

            // Centered child should also be centered at 0, 0
            Vector3 centerPos = previewObjects[1].transform.localPosition;
            Assert.AreEqual(0f, centerPos.x, 0.001f, "Centered UI element X position must be at 0");
            Assert.AreEqual(0f, centerPos.y, 0.001f, "Centered UI element Y position must be at 0");
        }

        [Test]
        public void RenderQueue_IncrementsPerLayer_ToGuaranteeDrawOrderEvenAtZeroExplosion()
        {
            _viewport.SetExplosionFactor(0f); // Zero explosion / coplanar

            var entry0 = new UIElementEntry { GlobalDrawIndex = 0, WorldRect = new Rect(0, 0, 200, 200) };
            var entry1 = new UIElementEntry { GlobalDrawIndex = 1, WorldRect = new Rect(0, 0, 200, 200) };
            var entry2 = new UIElementEntry { GlobalDrawIndex = 2, WorldRect = new Rect(0, 0, 200, 200) };

            var entries = new List<UIElementEntry> { entry0, entry1, entry2 };
            _viewport.RebuildFromEntries(entries);

            var field = typeof(UIPreview3DViewport).GetField("_previewObjects", BindingFlags.NonPublic | BindingFlags.Instance);
            var previewObjects = (List<GameObject>)field.GetValue(_viewport);

            Assert.IsNotNull(previewObjects);
            Assert.AreEqual(3, previewObjects.Count);

            var mr0 = previewObjects[0].GetComponent<MeshRenderer>();
            var mr1 = previewObjects[1].GetComponent<MeshRenderer>();
            var mr2 = previewObjects[2].GetComponent<MeshRenderer>();

            Assert.IsNotNull(mr0.sharedMaterial);
            Assert.IsNotNull(mr1.sharedMaterial);
            Assert.IsNotNull(mr2.sharedMaterial);

            // RenderQueue must strictly increase with DrawIndex so background renders first (3000) and foreground renders on top (3002)
            Assert.Less(mr0.sharedMaterial.renderQueue, mr1.sharedMaterial.renderQueue, "Layer 0 must have lower renderQueue than Layer 1");
            Assert.Less(mr1.sharedMaterial.renderQueue, mr2.sharedMaterial.renderQueue, "Layer 1 must have lower renderQueue than Layer 2");
            Assert.AreEqual(3000, mr0.sharedMaterial.renderQueue, "Layer 0 renderQueue must start at 3000");
            Assert.AreEqual(3001, mr1.sharedMaterial.renderQueue, "Layer 1 renderQueue must be 3001");
            Assert.AreEqual(3002, mr2.sharedMaterial.renderQueue, "Layer 2 renderQueue must be 3002");
        }
}
}
