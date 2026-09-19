using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIDepthInspector.Editor.Benchmark
{
    public static class UISyntheticSceneGenerator
    {
        public const string RootContainerName = "[Benchmark_Generated_UI]";

        static Sprite s_DefaultSprite;
        static Font s_DefaultFont;

        public static Sprite GetOrCreateDefaultSprite()
        {
            if (s_DefaultSprite == null)
            {
                var tex = Texture2D.whiteTexture;
                s_DefaultSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            return s_DefaultSprite;
        }

        public static Font GetOrCreateDefaultFont()
        {
            if (s_DefaultFont == null)
            {
                s_DefaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (s_DefaultFont == null)
                    s_DefaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            return s_DefaultFont;
        }

        /// <summary>
        /// Clears and tears down all benchmark generated UI containers cleanly.
        /// </summary>
        public static void ClearBenchmarkUI()
        {
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                if (scene.isLoaded)
                {
                    var roots = scene.GetRootGameObjects();
                    for (int i = 0; i < roots.Length; i++)
                    {
                        if (roots[i] != null && roots[i].name == RootContainerName)
                        {
                            UnityEngine.Object.DestroyImmediate(roots[i]);
                        }
                    }
                }
            }

            GameObject root;
            while ((root = GameObject.Find(RootContainerName)) != null)
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

#if UNITY_EDITOR
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < allObjects.Length; i++)
            {
                var go = allObjects[i];
                if (go != null && go.name == RootContainerName && go.transform.parent == null)
                {
                    if (EditorUtility.IsPersistent(go)) continue;
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }
#endif
        }

        /// <summary>
        /// Procedurally generates a deterministic uGUI hierarchy based on the preset configuration.
        /// </summary>
        public static GameObject Generate(UIBenchmarkPresetConfig config, int seed, out UIBenchmarkGroundTruth groundTruth)
        {
            ClearBenchmarkUI();

            if (config == null)
                config = UIBenchmarkPreset.GetConfig(UIBenchmarkPresetType.CleanReference);

            var rng = new System.Random(seed);

            var root = new GameObject(RootContainerName);

            groundTruth = new UIBenchmarkGroundTruth
            {
                benchmarkId = $"BENCH_{config.presetType}_{seed}",
                seed = seed,
                preset = config.presetType.ToString(),
                generatedAt = DateTime.UtcNow.ToString("o"),
                totalElements = 0,
                totalAnomalies = 0,
                anomalies = new List<InjectedAnomalyEntry>()
            };

            int canvasCount = Mathf.Max(1, config.canvasCount);

            // Primary Canvas
            var mainCanvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            mainCanvasGo.transform.SetParent(root.transform, false);
            var mainCanvas = mainCanvasGo.GetComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 0;
            var mainRt = mainCanvasGo.GetComponent<RectTransform>();
            mainRt.sizeDelta = new Vector2(1920, 1080);

            // Keep reference to buttons for anomaly injection targets
            GameObject settingsBtnGo = null;
            GameObject helpBtnGo = null;
            GameObject homeBtnGo = null;
            GameObject firstCardBtnGo = null;

            // 1. HUD_TopBar
            var hudGo = CreatePanel("HUD_TopBar", mainCanvasGo.transform, new Vector2(1920, 70), new Vector2(0, 505), new Color(0.15f, 0.15f, 0.18f, 1f), raycastTarget: false);
            var hudLayout = hudGo.AddComponent<HorizontalLayoutGroup>();
            hudLayout.childControlWidth = false;
            hudLayout.childControlHeight = false;
            hudLayout.childForceExpandWidth = false;
            hudLayout.childForceExpandHeight = false;
            hudLayout.spacing = 15;
            hudLayout.padding = new RectOffset(20, 20, 10, 10);

            CreateLabel("HUD_Title", hudGo.transform, new Vector2(300, 45), Vector2.zero, "Milfoy Benchmark UI", 18, raycastTarget: false);
            settingsBtnGo = CreateButton("Button_Settings", hudGo.transform, new Vector2(120, 45), Vector2.zero, "Settings", new Color(0.25f, 0.45f, 0.75f, 1f));
            helpBtnGo = CreateButton("Button_Help", hudGo.transform, new Vector2(100, 45), Vector2.zero, "Help", new Color(0.25f, 0.45f, 0.75f, 1f));

            // 2. Action_BottomBar
            var bottomGo = CreatePanel("Action_BottomBar", mainCanvasGo.transform, new Vector2(1920, 80), new Vector2(0, -500), new Color(0.12f, 0.12f, 0.14f, 1f), raycastTarget: false);
            var bottomLayout = bottomGo.AddComponent<HorizontalLayoutGroup>();
            bottomLayout.childControlWidth = false;
            bottomLayout.childControlHeight = false;
            bottomLayout.childForceExpandWidth = false;
            bottomLayout.childForceExpandHeight = false;
            bottomLayout.spacing = 20;
            bottomLayout.padding = new RectOffset(30, 30, 12, 12);

            homeBtnGo = CreateButton("Button_Home", bottomGo.transform, new Vector2(130, 50), Vector2.zero, "Home", new Color(0.28f, 0.58f, 0.35f, 1f));
            CreateButton("Button_Inventory", bottomGo.transform, new Vector2(130, 50), Vector2.zero, "Inventory", new Color(0.28f, 0.58f, 0.35f, 1f));
            CreateButton("Button_Shop", bottomGo.transform, new Vector2(130, 50), Vector2.zero, "Shop", new Color(0.28f, 0.58f, 0.35f, 1f));
            CreateButton("Button_Profile", bottomGo.transform, new Vector2(130, 50), Vector2.zero, "Profile", new Color(0.28f, 0.58f, 0.35f, 1f));

            // 3. Content_Panel
            var contentGo = CreatePanel("Content_Panel", mainCanvasGo.transform, new Vector2(1400, 650), new Vector2(0, 10), new Color(0.18f, 0.18f, 0.22f, 0.95f), raycastTarget: false);
            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.childControlWidth = false;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childForceExpandHeight = false;
            contentLayout.spacing = 15;
            contentLayout.padding = new RectOffset(25, 25, 20, 20);

            // Estimate card count based on targetElementCount
            int currentBaseElements = 14;
            int remainingElements = Mathf.Max(6, config.targetElementCount - currentBaseElements);
            int cardCount = Mathf.Clamp(remainingElements / 5, 1, 12);

            for (int i = 0; i < cardCount; i++)
            {
                var cardGo = CreatePanel($"Card_{i}", contentGo.transform, new Vector2(1350, 75), Vector2.zero, new Color(0.22f, 0.22f, 0.28f, 1f), raycastTarget: false);
                var cardLayout = cardGo.AddComponent<HorizontalLayoutGroup>();
                cardLayout.childControlWidth = false;
                cardLayout.childControlHeight = false;
                cardLayout.childForceExpandWidth = false;
                cardLayout.childForceExpandHeight = false;
                cardLayout.spacing = 15;
                cardLayout.padding = new RectOffset(15, 15, 12, 12);

                CreateLabel($"Card_{i}_Title", cardGo.transform, new Vector2(300, 45), Vector2.zero, $"Inspection Target #{i + 1}", 16, raycastTarget: false);
                var actBtn = CreateButton($"Button_Action_{i}", cardGo.transform, new Vector2(130, 45), Vector2.zero, "Inspect", new Color(0.4f, 0.35f, 0.65f, 1f));
                if (firstCardBtnGo == null)
                    firstCardBtnGo = actBtn;
            }

            // Secondary Canvas (if configured)
            GameObject overlayCanvasGo = null;
            if (canvasCount > 1)
            {
                overlayCanvasGo = new GameObject("Canvas_Overlay", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                overlayCanvasGo.transform.SetParent(root.transform, false);
                var overlayCanvas = overlayCanvasGo.GetComponent<Canvas>();
                overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                overlayCanvas.sortingOrder = 20;
                var overlayRt = overlayCanvasGo.GetComponent<RectTransform>();
                overlayRt.sizeDelta = new Vector2(1920, 1080);

                var modalGo = CreatePanel("Modal_Popup", overlayCanvasGo.transform, new Vector2(600, 360), Vector2.zero, new Color(0.25f, 0.25f, 0.3f, 1f), raycastTarget: false);
                CreateLabel("Modal_Title", modalGo.transform, new Vector2(500, 50), new Vector2(0, 130), "Production Notice", 18, raycastTarget: false);
                CreateButton("Button_ModalConfirm", modalGo.transform, new Vector2(140, 45), new Vector2(-90, -120), "Acknowledge", new Color(0.3f, 0.6f, 0.4f, 1f));
                CreateButton("Button_ModalClose", modalGo.transform, new Vector2(140, 45), new Vector2(90, -120), "Dismiss", new Color(0.6f, 0.3f, 0.3f, 1f));
            }

            // ================== ANOMALY INJECTION ==================

            // 1. Ghost Blocker
            if (config.injectGhostBlockers && settingsBtnGo != null)
            {
                var ghostGo = new GameObject("Ghost_Blocker", typeof(RectTransform), typeof(Image));
                ghostGo.transform.SetParent(hudGo.transform, false);
                var gRt = ghostGo.GetComponent<RectTransform>();
                var targetRt = settingsBtnGo.GetComponent<RectTransform>();
                gRt.sizeDelta = targetRt.sizeDelta;
                gRt.anchoredPosition = targetRt.anchoredPosition;

                var gImg = ghostGo.GetComponent<Image>();
                gImg.sprite = GetOrCreateDefaultSprite();
                gImg.color = new Color(1f, 1f, 1f, 0f); // Completely invisible
                gImg.raycastTarget = true; // Still intercepts raycasts!

                groundTruth.anomalies.Add(new InjectedAnomalyEntry
                {
                    anomalyId = $"ANO_{groundTruth.anomalies.Count + 1:D3}",
                    type = "ANOMALY_GHOST_BLOCKER",
                    targetPath = GetHierarchyPath(ghostGo.transform, root.transform),
                    component = "Image",
                    property = "raycastTarget",
                    injectedValue = "true",
                    expectedValue = "false",
                    affectedTarget = GetHierarchyPath(settingsBtnGo.transform, root.transform)
                });
            }

            // 2. Spatial Overlap Blocker
            if (config.injectSpatialOverlaps && homeBtnGo != null)
            {
                Transform parentTransform = overlayCanvasGo != null ? overlayCanvasGo.transform : mainCanvasGo.transform;
                var overlapGo = new GameObject("Overlap_Blocker", typeof(RectTransform), typeof(Image));
                overlapGo.transform.SetParent(parentTransform, false);

                var oRt = overlapGo.GetComponent<RectTransform>();
                var homeRt = homeBtnGo.GetComponent<RectTransform>();
                oRt.sizeDelta = new Vector2(homeRt.sizeDelta.x + 20, homeRt.sizeDelta.y + 20);
                oRt.position = homeRt.position; // Align world positions

                var oImg = overlapGo.GetComponent<Image>();
                oImg.sprite = GetOrCreateDefaultSprite();
                oImg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f); // Visible semi-transparent blocker
                oImg.raycastTarget = true;

                groundTruth.anomalies.Add(new InjectedAnomalyEntry
                {
                    anomalyId = $"ANO_{groundTruth.anomalies.Count + 1:D3}",
                    type = "ANOMALY_SPATIAL_OVERLAP",
                    targetPath = GetHierarchyPath(overlapGo.transform, root.transform),
                    component = "Image",
                    property = "raycastTarget",
                    injectedValue = "true",
                    expectedValue = "false",
                    affectedTarget = GetHierarchyPath(homeBtnGo.transform, root.transform)
                });
            }

            // 3. Missing Sprite / Zero-Size Rect
            if (config.injectMissingSprites)
            {
                var missingGo = new GameObject("Missing_Sprite_Icon", typeof(RectTransform), typeof(Image));
                missingGo.transform.SetParent(contentGo.transform, false);
                var mRt = missingGo.GetComponent<RectTransform>();
                mRt.sizeDelta = new Vector2(40, 40);

                var mImg = missingGo.GetComponent<Image>();
                mImg.sprite = null; // Missing sprite!
                mImg.color = Color.white;
                mImg.raycastTarget = true;

                groundTruth.anomalies.Add(new InjectedAnomalyEntry
                {
                    anomalyId = $"ANO_{groundTruth.anomalies.Count + 1:D3}",
                    type = "ANOMALY_MISSING_SPRITE",
                    targetPath = GetHierarchyPath(missingGo.transform, root.transform),
                    component = "Image",
                    property = "sprite",
                    injectedValue = "null",
                    expectedValue = "assigned",
                    affectedTarget = ""
                });
            }

            // 4. Nested Label Raycast Trap
            if (config.injectNestedLabelRaycasts && helpBtnGo != null)
            {
                var labelText = helpBtnGo.GetComponentInChildren<Text>();
                if (labelText != null)
                {
                    labelText.raycastTarget = true; // Redundant nested label interceptor!

                    groundTruth.anomalies.Add(new InjectedAnomalyEntry
                    {
                        anomalyId = $"ANO_{groundTruth.anomalies.Count + 1:D3}",
                        type = "ANOMALY_NESTED_LABEL_RAYCAST",
                        targetPath = GetHierarchyPath(labelText.transform, root.transform),
                        component = "Text",
                        property = "raycastTarget",
                        injectedValue = "true",
                        expectedValue = "false",
                        affectedTarget = GetHierarchyPath(helpBtnGo.transform, root.transform)
                    });
                }
            }

            // 5. CanvasGroup Trap
            if (config.injectCanvasGroupTraps)
            {
                var trapGo = new GameObject("CanvasGroup_Trap", typeof(RectTransform), typeof(CanvasGroup));
                trapGo.transform.SetParent(contentGo.transform, false);
                var cg = trapGo.GetComponent<CanvasGroup>();
                cg.alpha = 0f; // Completely invisible container
                cg.blocksRaycasts = true; // But still intercepts input!

                var trappedBtn = CreateButton("Button_Trapped", trapGo.transform, new Vector2(130, 45), Vector2.zero, "Hidden Trap", Color.gray);

                groundTruth.anomalies.Add(new InjectedAnomalyEntry
                {
                    anomalyId = $"ANO_{groundTruth.anomalies.Count + 1:D3}",
                    type = "ANOMALY_CANVASGROUP_TRAP",
                    targetPath = GetHierarchyPath(trapGo.transform, root.transform),
                    component = "CanvasGroup",
                    property = "blocksRaycasts",
                    injectedValue = "true",
                    expectedValue = "false",
                    affectedTarget = GetHierarchyPath(trappedBtn.transform, root.transform)
                });
            }

            // Finalize ground truth stats
            groundTruth.totalAnomalies = groundTruth.anomalies.Count;
            groundTruth.totalElements = root.GetComponentsInChildren<RectTransform>(true).Length;

            return root;
        }

        public static string GetHierarchyPath(Transform t, Transform stopAt)
        {
            if (t == null || t == stopAt) return "";
            string path = t.name;
            Transform curr = t.parent;
            while (curr != null && curr != stopAt)
            {
                path = curr.name + "/" + path;
                curr = curr.parent;
            }
            return path;
        }

        static GameObject CreatePanel(string name, Transform parent, Vector2 size, Vector2 anchoredPos, Color color, bool raycastTarget = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            var img = go.GetComponent<Image>();
            img.sprite = GetOrCreateDefaultSprite();
            img.color = color;
            img.raycastTarget = raycastTarget;
            return go;
        }

        static GameObject CreateButton(string name, Transform parent, Vector2 size, Vector2 anchoredPos, string labelText, Color btnColor, bool labelRaycast = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            var img = go.GetComponent<Image>();
            img.sprite = GetOrCreateDefaultSprite();
            img.color = btnColor;
            img.raycastTarget = true;

            var labelGo = new GameObject(name + "_Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.sizeDelta = size;
            labelRt.anchoredPosition = Vector2.zero;
            var txt = labelGo.GetComponent<Text>();
            txt.font = GetOrCreateDefaultFont();
            txt.text = labelText;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.raycastTarget = labelRaycast;

            return go;
        }

        static GameObject CreateLabel(string name, Transform parent, Vector2 size, Vector2 anchoredPos, string content, int fontSize = 14, bool raycastTarget = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            var txt = go.GetComponent<Text>();
            txt.font = GetOrCreateDefaultFont();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.raycastTarget = raycastTarget;
            return go;
        }
    }
}
