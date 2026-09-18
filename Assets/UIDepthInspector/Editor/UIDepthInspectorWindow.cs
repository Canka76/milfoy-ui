using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UIDepthInspector.Editor
{
    using Core;
    using Diagnostics;
    using Panels;
    using Viewport;

    public class UIDepthInspectorWindow : EditorWindow
    {
        [MenuItem("Window/UI/UI Depth Inspector")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<UIDepthInspectorWindow>();
            wnd.UpdateTitleContent();
            wnd.minSize = new Vector2(600, 400);
        }

        UIRenderTreeCache _cache;
        UIPreview3DViewport _viewport;
        UIToolbarPanel _toolbar;
        UIStackListPanel _listPanel;

        int _lastCacheGeneration = -1;
        bool _needsRepaint;

        // Solo state
        struct SoloState
        {
            public bool Active;
            public Transform Parent;
            public List<(GameObject go, bool wasActive)> Saved;
        }
        SoloState _solo;

        void OnEnable()
        {
            UpdateTitleContent();
            _cache = new UIRenderTreeCache();
            _viewport = new UIPreview3DViewport();
            _viewport.Initialize();
            rootVisualElement.style.flexGrow = 1;
            rootVisualElement.style.height = Length.Percent(100);
            rootVisualElement.style.width = Length.Percent(100);

            // Load UXML & USS reliably in Editor
            var visualTree = LoadEditorAsset<VisualTreeAsset>("UIDepthInspector", "uxml");
            if (visualTree != null)
                visualTree.CloneTree(rootVisualElement);

            var stylesheet = LoadEditorAsset<StyleSheet>("UIDepthInspector", "uss");
            if (stylesheet != null)
                rootVisualElement.styleSheets.Add(stylesheet);

            var splitView = rootVisualElement.Q<TwoPaneSplitView>("split-view");
            if (splitView != null)
            {
                splitView.style.flexGrow = 1;
                splitView.style.height = Length.Percent(100);
            }

            var listPane = rootVisualElement.Q("list-pane");
            if (listPane != null)
            {
                listPane.style.flexDirection = FlexDirection.Column;
                listPane.style.minWidth = 340;
                listPane.style.flexGrow = 1;
            }

            var viewportContainerElem = rootVisualElement.Q("viewport-container");
            if (viewportContainerElem != null)
            {
                viewportContainerElem.style.minWidth = 200;
            }

            var header = rootVisualElement.Q("window-header");
            if (header != null)
            {
                header.style.flexDirection = FlexDirection.Row;
                header.style.alignItems = Align.Center;
                header.style.paddingLeft = 8;
                header.style.paddingRight = 8;
                header.style.paddingTop = 6;
                header.style.paddingBottom = 6;
                header.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f);
                header.style.borderBottomWidth = 1;
                header.style.borderBottomColor = new Color(0.11f, 0.11f, 0.11f, 1f);
            }

            var logo = rootVisualElement.Q("header-logo");
            if (logo != null)
            {
                logo.style.width = 16;
                logo.style.height = 16;
                logo.style.minWidth = 16;
                logo.style.minHeight = 16;
                logo.style.borderTopLeftRadius = 4;
                logo.style.borderTopRightRadius = 4;
                logo.style.borderBottomLeftRadius = 4;
                logo.style.borderBottomRightRadius = 4;
                logo.style.backgroundColor = new Color(0.38f, 0.63f, 0.88f, 1f);
                logo.style.marginRight = 6;
            }

            var statusBarElem = rootVisualElement.Q<Label>("status-bar");
            if (statusBarElem != null)
            {
                statusBarElem.style.paddingLeft = 8;
                statusBarElem.style.paddingRight = 8;
                statusBarElem.style.paddingTop = 3;
                statusBarElem.style.paddingBottom = 3;
                statusBarElem.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
                statusBarElem.style.borderTopWidth = 1;
                statusBarElem.style.borderTopColor = new Color(0.12f, 0.12f, 0.12f, 1f);
                statusBarElem.style.color = new Color(0.65f, 0.65f, 0.65f, 1f);
                statusBarElem.style.fontSize = 11;
            }

            // Setup toolbar
            _toolbar = new UIToolbarPanel();
            _toolbar.Bind(rootVisualElement);
            _toolbar.OnViewPresetChanged += preset =>
            {
                _viewport.SetViewPreset(preset);
                _needsRepaint = true;
            };
            _toolbar.OnExplosionChanged += factor =>
            {
                _viewport.SetExplosionFactor(factor);
                _needsRepaint = true;
            };
            _toolbar.OnFilterChanged += () =>
            {
                _listPanel.ApplyFilters(_toolbar);
                RebuildViewportFromFiltered();
                _needsRepaint = true;
            };

            // Setup list
            _listPanel = new UIStackListPanel();
            _listPanel.Bind(rootVisualElement);
            _listPanel.OnEntryClicked += entry =>
            {
                if (entry.Transform != null)
                    Selection.activeGameObject = entry.Transform.gameObject;
            };
            _listPanel.OnActiveToggled += ToggleActive;
            _listPanel.OnRaycastToggled += ToggleRaycast;
            _listPanel.OnSoloToggled += ToggleSolo;

            // Setup viewport IMGUI container
            var viewportContainer = rootVisualElement.Q<IMGUIContainer>("viewport-container");
            if (viewportContainer != null)
            {
                viewportContainer.onGUIHandler = () =>
                {
                    var rect = viewportContainer.contentRect;
                    var evt = Event.current;

                    // Handle interactive inputs (mouse drag orbit, zoom, pan) on any layout/input event
                    if (rect.width > 1 && rect.height > 1)
                    {
                        _viewport.HandleInput(evt, rect);

                        // Left-click picking
                        if (evt.type == EventType.MouseDown && evt.button == 0 && !evt.alt)
                        {
                            int picked = _viewport.GetPickedEntryIndex(evt.mousePosition, rect);
                            if (picked >= 0)
                            {
                                var entries = _cache.Entries;
                                if (picked < entries.Count && entries[picked].Transform != null)
                                    Selection.activeGameObject = entries[picked].Transform.gameObject;
                                evt.Use();
                            }
                        }

                        // Render 3D preview viewport (automatically guards EventType.Repaint internally)
                        _viewport.OnGUI(rect);
                    }

                    if (evt.type == EventType.MouseDrag || evt.type == EventType.ScrollWheel)
                        _needsRepaint = true;
                };
            }
            // Selection sync
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.update += OnEditorUpdate;

            // Initial view preset
            _viewport.SetViewPreset(ViewPreset.Isometric);

            // Force initial rebuild
            _cache.Invalidate();
        }

        void OnDisable()
        {
            RestoreSolo();

            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.update -= OnEditorUpdate;

            _cache?.Dispose();
            _viewport?.Dispose();
        }

        void OnEditorUpdate()
        {
            // Check if cache was rebuilt
            if (_cache.Generation != _lastCacheGeneration)
            {
                _lastCacheGeneration = _cache.Generation;
                var entries = new List<UIElementEntry>(_cache.Entries);
                UIDiagnosticAnalyzer.Analyze(entries);

                _toolbar.PopulateCanvasDropdown(entries);
                _listPanel.SetEntries(entries, _toolbar);
                _viewport.RebuildFromEntries(entries);

                UpdateStatusBar(entries);
                _needsRepaint = true;
            }

            if (_needsRepaint)
            {
                _needsRepaint = false;
                Repaint();
            }
        }

        void OnSelectionChanged()
        {
            var selected = Selection.activeGameObject;
            if (selected == null) return;

            var entries = _cache.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Transform != null && entries[i].Transform.gameObject == selected)
                {
                    _viewport.HighlightEntry(entries[i].GlobalDrawIndex);
                    _listPanel.SelectEntry(entries[i].GlobalDrawIndex);
                    _needsRepaint = true;
                    return;
                }
            }

            _viewport.HighlightEntry(-1);
        }

        void UpdateStatusBar(List<UIElementEntry> entries)
        {
            int warnings = 0;
            var logBuilder = new System.Text.StringBuilder();

            foreach (var e in entries)
            {
                if ((e.Flags & DiagnosticFlags.GhostBlocker) != 0)
                {
                    warnings++;
                    string reason = (e.EffectiveAlpha <= 0f)
                        ? "Alpha is 0 (fully transparent) while Raycast Target is active."
                        : "Image sprite is missing (null) while Raycast Target is active.";
                    logBuilder.AppendLine($"⚠ [#{e.GlobalDrawIndex:D2} {e.Name}]: {reason}");
                }
                else if ((e.Flags & DiagnosticFlags.ZeroSize) != 0)
                {
                    logBuilder.AppendLine($"ℹ [#{e.GlobalDrawIndex:D2} {e.Name}]: RectTransform width/height is near zero.");
                }
            }

            if (warnings == 0 && logBuilder.Length == 0)
            {
                logBuilder.Append("✓ All UI layers healthy. No invisible blockers or occlusions detected.");
            }

            var logText = rootVisualElement.Q<Label>("log-text");
            if (logText != null)
            {
                logText.text = logBuilder.ToString().TrimEnd();
                logText.style.color = (warnings > 0) ? new Color(1f, 0.75f, 0.25f, 1f) : new Color(0.6f, 0.85f, 0.6f, 1f);
            }

            var statusBar = rootVisualElement.Q<Label>("status-bar");
            if (statusBar != null)
            {
                string canvas = entries.Count > 0 ? entries[0].RootCanvasName : "—";
                statusBar.text = $"{entries.Count} elements │ {warnings} warnings │ Canvas: {canvas}";
            }
        }

        void ToggleActive(UIElementEntry entry)
        {
            if (entry.Transform == null) return;
            var go = entry.Transform.gameObject;
            Undo.RecordObject(go, "Toggle Active");
            go.SetActive(!go.activeSelf);
            _cache.Invalidate();
        }

        void ToggleRaycast(UIElementEntry entry)
        {
            if (entry.Transform == null) return;
            if (!entry.Transform.TryGetComponent<Graphic>(out var graphic)) return;
            Undo.RecordObject(graphic, "Toggle Raycast");
            graphic.raycastTarget = !graphic.raycastTarget;
            _cache.Invalidate();
        }

        void ToggleSolo(UIElementEntry entry)
        {
            if (entry.Transform == null) return;

            if (_solo.Active)
            {
                RestoreSolo();
                _cache.Invalidate();
                return;
            }

            var parent = entry.Transform.parent;
            if (parent == null) return;

            _solo.Active = true;
            _solo.Parent = parent;
            _solo.Saved = new List<(GameObject, bool)>();

            Undo.SetCurrentGroupName("Solo UI Element");
            var graphics = parent.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                if (g.transform == entry.Transform) continue;
                var go = g.gameObject;
                _solo.Saved.Add((go, go.activeSelf));
                Undo.RecordObject(go, "Solo UI Element");
                go.SetActive(false);
            }

            _cache.Invalidate();
        }

        void RestoreSolo()
        {
            if (!_solo.Active || _solo.Saved == null) return;

            foreach (var (go, wasActive) in _solo.Saved)
            {
                if (go != null)
                {
                    Undo.RecordObject(go, "Restore Solo");
                    go.SetActive(wasActive);
                }
            }

            _solo.Active = false;
            _solo.Saved = null;
        }

        // Keyboard shortcuts
        [Shortcut("UIDepthInspector/ExitSolo", KeyCode.BackQuote)]
        static void ExitSoloShortcut()
        {
            var wnd = GetWindow<UIDepthInspectorWindow>();
            if (wnd != null && wnd._solo.Active)
            {
                wnd.RestoreSolo();
                wnd._cache.Invalidate();
            }
        }

        [Shortcut("UIDepthInspector/FrameSelected", KeyCode.F)]
        static void FrameSelectedShortcut()
        {
            var wnd = GetWindow<UIDepthInspectorWindow>();
            if (wnd == null) return;

            var selected = Selection.activeGameObject;
            if (selected == null) return;

            var entries = wnd._cache.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Transform != null && entries[i].Transform.gameObject == selected)
                {
                    wnd._viewport.FrameEntry(entries[i].GlobalDrawIndex);
                    wnd._needsRepaint = true;
                    return;
                }
            }
        }

        void RebuildViewportFromFiltered()
        {
            // Viewport shows all entries; filtering only affects list visibility
        }

        void UpdateTitleContent()
        {
            // 1. Try loading custom icon asset
            var customIcon = LoadEditorAsset<Texture2D>("MilfoyIcon", "png");

            // 2. If no custom icon asset yet, generate a crisp 16x16 3D layer icon
            if (customIcon == null)
            {
                customIcon = CreateTabIconTexture();
            }

            titleContent = new GUIContent("Milfoy", customIcon, "Milfoy — UI Layer & 3D Depth Inspector");
        }

        static Texture2D s_CachedTabIcon;
        static Texture2D CreateTabIconTexture()
        {
            if (s_CachedTabIcon != null) return s_CachedTabIcon;

            s_CachedTabIcon = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            s_CachedTabIcon.hideFlags = HideFlags.HideAndDontSave;

            var clear = new Color(0, 0, 0, 0);
            var cyan = new Color(0.38f, 0.65f, 0.95f, 1f); // #60A0E0
            var white = new Color(0.9f, 0.95f, 1f, 1f);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    s_CachedTabIcon.SetPixel(x, y, clear);
                }
            }

            // Draw 3 layered isometric / stacked rectangles (symbolizing 3D UI Depth layers)
            // Bottom layer
            for (int x = 2; x <= 13; x++) { s_CachedTabIcon.SetPixel(x, 3, cyan); s_CachedTabIcon.SetPixel(x, 4, cyan); }
            // Middle layer
            for (int x = 3; x <= 12; x++) { s_CachedTabIcon.SetPixel(x, 7, cyan); s_CachedTabIcon.SetPixel(x, 8, cyan); }
            // Top layer (highlighted)
            for (int x = 4; x <= 11; x++) { s_CachedTabIcon.SetPixel(x, 11, white); s_CachedTabIcon.SetPixel(x, 12, white); }

            s_CachedTabIcon.Apply();
            return s_CachedTabIcon;
        }

        static T LoadEditorAsset<T>(string filenameWithoutExt, string extension) where T : UnityEngine.Object
        {
            // 1. Try Resources.Load
            var res = Resources.Load<T>(filenameWithoutExt);
            if (res != null) return res;

            // 2. Try AssetDatabase search
            string[] guids = AssetDatabase.FindAssets($"{filenameWithoutExt} t:{typeof(T).Name}");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith($".{extension}", System.StringComparison.OrdinalIgnoreCase))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                    if (asset != null) return asset;
                }
            }

            // 3. Fallback known paths
            string[] fallbackPaths = new[]
            {
                $"Assets/UIDepthInspector/Editor/Resources/{filenameWithoutExt}.{extension}",
                $"Packages/com.openupm.ui-depth-inspector/Editor/Resources/{filenameWithoutExt}.{extension}"
            };
            foreach (var p in fallbackPaths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(p);
                if (asset != null) return asset;
            }

            return null;
        }
    }
}
