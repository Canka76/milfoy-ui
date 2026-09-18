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
    }
}
