# Task 9 Brief: EditorWindow — Wire Everything Together

## Files to Create
- `Editor/UIDepthInspectorWindow.cs`

## Exact Contents

```csharp
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
            wnd.titleContent = new GUIContent("UI Depth Inspector");
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
            _cache = new UIRenderTreeCache();
            _viewport = new UIPreview3DViewport();
            _viewport.Initialize();

            // Load UXML
            var visualTree = Resources.Load<VisualTreeAsset>("UIDepthInspector");
            if (visualTree != null)
                visualTree.CloneTree(rootVisualElement);

            // Load USS
            var stylesheet = Resources.Load<StyleSheet>("UIDepthInspector");
            if (stylesheet != null)
                rootVisualElement.styleSheets.Add(stylesheet);

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
                    if (rect.width < 1 || rect.height < 1) return;

                    var evt = Event.current;
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

                    _viewport.OnGUI(rect);

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
            foreach (var e in entries)
            {
                if ((e.Flags & DiagnosticFlags.GhostBlocker) != 0)
                    warnings++;
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
        [Shortcut("UIDepthInspector/ExitSolo", KeyCode.Escape)]
        static void ExitSoloShortcut()
        {
            var wnd = GetWindow<UIDepthInspectorWindow>();
            if (wnd != null && wnd._solo.Active)
            {
                wnd.RestoreSolo();
                wnd._cache.Invalidate();
            }
        }

        void RebuildViewportFromFiltered()
        {
            // Viewport shows all entries; filtering only affects list visibility
        }
    }
}
```

## Instructions
1. Create `Editor/UIDepthInspectorWindow.cs` with the exact C# code specified.
2. Ensure proper formatting and LF line endings.
3. Stage with `git add Editor/UIDepthInspectorWindow.cs`.
4. Commit with message: `feat: wire EditorWindow with viewport, panels, selection sync, and solo mode`.
5. Write report to `.superpowers/sdd/task-9-report.md`.
