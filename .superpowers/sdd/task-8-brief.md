# Task 8 Brief: UXML/USS Layout, Toolbar Panel, Stack List Panel

## Files to Create
- `Editor/Resources/UIDepthInspector.uxml`
- `Editor/Resources/UIDepthInspector.uss`
- `Editor/Diagnostics/UIDiagnosticBadges.cs`
- `Editor/Panels/UIToolbarPanel.cs`
- `Editor/Panels/UIStackListPanel.cs`

## Exact Contents

### `Editor/Resources/UIDepthInspector.uxml`
```xml
<?xml version="1.0" encoding="utf-8"?>
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements">
    <ui:VisualElement name="root" class="root">
        <!-- Toolbar -->
        <ui:VisualElement name="toolbar" class="toolbar">
            <ui:VisualElement class="toolbar-row">
                <ui:Button name="btn-front" text="Front" class="preset-btn" />
                <ui:Button name="btn-iso" text="Iso" class="preset-btn" />
                <ui:Button name="btn-side" text="Side" class="preset-btn" />
                <ui:VisualElement class="toolbar-separator" />
                <ui:Label text="Z-Explosion:" class="slider-label" />
                <ui:Slider name="slider-explosion" low-value="0" high-value="50" value="0" class="explosion-slider" />
                <ui:FloatField name="field-explosion" value="0" class="explosion-field" />
            </ui:VisualElement>
            <ui:VisualElement class="toolbar-row">
                <ui:TextField name="search-field" class="search-field" />
                <ui:Toggle name="toggle-raycast" text="Raycast Only" class="filter-toggle" />
                <ui:Toggle name="toggle-warnings" text="Warnings" class="filter-toggle" />
                <ui:Toggle name="toggle-active" text="Active Only" class="filter-toggle" />
            </ui:VisualElement>
        </ui:VisualElement>

        <!-- Main content split -->
        <ui:TwoPaneSplitView name="split-view" fixed-pane-index="1" fixed-pane-initial-dimension="300" orientation="Horizontal">
            <!-- Left: 3D Viewport (IMGUI) -->
            <ui:IMGUIContainer name="viewport-container" class="viewport" />

            <!-- Right: Stack list -->
            <ui:VisualElement name="list-pane" class="list-pane">
                <ui:ListView name="stack-list" class="stack-list" />
                <ui:VisualElement class="list-footer">
                    <ui:DropdownField name="canvas-filter" label="Canvas" class="canvas-dropdown" />
                    <ui:Button name="btn-copy-stack" text="Copy Stack" class="copy-btn" />
                </ui:VisualElement>
            </ui:VisualElement>
        </ui:TwoPaneSplitView>

        <!-- Status bar -->
        <ui:Label name="status-bar" text="No Canvas found" class="status-bar" />
    </ui:VisualElement>
</ui:UXML>
```

### `Editor/Resources/UIDepthInspector.uss`
```css
.root {
    flex-grow: 1;
    flex-direction: column;
}

.toolbar {
    padding: 4px;
    background-color: #383838;
    border-bottom-width: 1px;
    border-bottom-color: #222;
}

.toolbar-row {
    flex-direction: row;
    align-items: center;
    margin-bottom: 2px;
}

.preset-btn {
    width: 50px;
    margin-right: 2px;
}

.toolbar-separator {
    width: 1px;
    height: 18px;
    background-color: #555;
    margin-left: 6px;
    margin-right: 6px;
}

.slider-label {
    margin-right: 4px;
    -unity-font-style: bold;
    font-size: 11px;
}

.explosion-slider {
    flex-grow: 1;
    min-width: 100px;
}

.explosion-field {
    width: 50px;
    margin-left: 4px;
}

.search-field {
    flex-grow: 1;
    margin-right: 4px;
}

.search-field > .unity-text-field__input {
    padding-left: 4px;
}

.filter-toggle {
    margin-right: 6px;
    font-size: 11px;
}

.viewport {
    flex-grow: 1;
    min-width: 200px;
}

.list-pane {
    flex-direction: column;
}

.stack-list {
    flex-grow: 1;
}

.list-footer {
    flex-direction: row;
    padding: 4px;
    border-top-width: 1px;
    border-top-color: #222;
    align-items: center;
}

.canvas-dropdown {
    flex-grow: 1;
    margin-right: 4px;
}

.copy-btn {
    width: 90px;
}

.status-bar {
    padding: 2px 6px;
    background-color: #2a2a2a;
    font-size: 11px;
    color: #aaa;
    border-top-width: 1px;
    border-top-color: #222;
}

/* Stack list row styles */
.stack-row {
    flex-direction: row;
    align-items: center;
    padding: 2px 4px;
    border-bottom-width: 1px;
    border-bottom-color: #333;
}

.stack-row:hover {
    background-color: #3a3a3a;
}

.stack-row--selected {
    background-color: #2c5d87;
}

.stack-index {
    width: 32px;
    font-size: 10px;
    -unity-font-style: bold;
    color: #888;
    -unity-text-align: middle-right;
    margin-right: 4px;
}

.stack-dot {
    width: 10px;
    height: 10px;
    border-radius: 5px;
    margin-right: 6px;
}

.stack-dot--raycast { background-color: #E06060; }
.stack-dot--passive { background-color: #60A0E0; }
.stack-dot--ghost   { background-color: #FFB030; }
.stack-dot--inactive { background-color: #808080; }

.stack-name {
    flex-grow: 1;
    font-size: 12px;
    overflow: hidden;
    text-overflow: ellipsis;
}

.stack-name--inactive {
    color: #666;
}

.warning-badge {
    font-size: 10px;
    color: #FFB030;
    margin-right: 4px;
    -unity-font-style: bold;
}

.row-btn {
    width: 22px;
    height: 22px;
    margin-left: 1px;
    font-size: 12px;
    padding: 0;
    -unity-text-align: middle-center;
}
```

### `Editor/Diagnostics/UIDiagnosticBadges.cs`
```csharp
using UnityEngine.UIElements;

namespace UIDepthInspector.Editor.Diagnostics
{
    using Core;

    public static class UIDiagnosticBadges
    {
        public static VisualElement CreateDot(DiagnosticFlags flags)
        {
            var dot = new VisualElement();
            dot.AddToClassList("stack-dot");

            if ((flags & DiagnosticFlags.Inactive) != 0)
                dot.AddToClassList("stack-dot--inactive");
            else if ((flags & DiagnosticFlags.GhostBlocker) != 0)
                dot.AddToClassList("stack-dot--ghost");
            else if ((flags & DiagnosticFlags.RaycastBlocker) != 0)
                dot.AddToClassList("stack-dot--raycast");
            else
                dot.AddToClassList("stack-dot--passive");

            return dot;
        }

        public static Label CreateWarningBadge()
        {
            return new Label("⚠ Invisible Hitbox") { pickingMode = PickingMode.Ignore };
        }
    }
}
```

### `Editor/Panels/UIToolbarPanel.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace UIDepthInspector.Editor.Panels
{
    using Core;
    using Viewport;

    public class UIToolbarPanel
    {
        public Action<ViewPreset> OnViewPresetChanged;
        public Action<float> OnExplosionChanged;
        public Action OnFilterChanged;

        public string SearchQuery { get; private set; } = "";
        public bool RaycastOnly { get; private set; }
        public bool WarningsOnly { get; private set; }
        public bool ActiveOnly { get; private set; }
        public int CanvasFilterId { get; private set; } = -1; // -1 = All

        Slider _slider;
        FloatField _floatField;
        DropdownField _canvasDropdown;
        readonly List<(int id, string name)> _canvasOptions = new();

        public void Bind(VisualElement root)
        {
            root.Q<Button>("btn-front").clicked += () => OnViewPresetChanged?.Invoke(ViewPreset.Front);
            root.Q<Button>("btn-iso").clicked += () => OnViewPresetChanged?.Invoke(ViewPreset.Isometric);
            root.Q<Button>("btn-side").clicked += () => OnViewPresetChanged?.Invoke(ViewPreset.Side);

            _slider = root.Q<Slider>("slider-explosion");
            _floatField = root.Q<FloatField>("field-explosion");

            _slider.RegisterValueChangedCallback(evt =>
            {
                _floatField.SetValueWithoutNotify(evt.newValue);
                OnExplosionChanged?.Invoke(evt.newValue);
            });

            _floatField.RegisterValueChangedCallback(evt =>
            {
                float clamped = UnityEngine.Mathf.Clamp(evt.newValue, 0f, 50f);
                _slider.SetValueWithoutNotify(clamped);
                _floatField.SetValueWithoutNotify(clamped);
                OnExplosionChanged?.Invoke(clamped);
            });

            root.Q<TextField>("search-field").RegisterValueChangedCallback(evt =>
            {
                SearchQuery = evt.newValue ?? "";
                OnFilterChanged?.Invoke();
            });

            root.Q<Toggle>("toggle-raycast").RegisterValueChangedCallback(evt =>
            {
                RaycastOnly = evt.newValue;
                OnFilterChanged?.Invoke();
            });

            root.Q<Toggle>("toggle-warnings").RegisterValueChangedCallback(evt =>
            {
                WarningsOnly = evt.newValue;
                OnFilterChanged?.Invoke();
            });

            root.Q<Toggle>("toggle-active").RegisterValueChangedCallback(evt =>
            {
                ActiveOnly = evt.newValue;
                OnFilterChanged?.Invoke();
            });

            _canvasDropdown = root.Q<DropdownField>("canvas-filter");
            _canvasDropdown.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == "All")
                    CanvasFilterId = -1;
                else
                {
                    var match = _canvasOptions.FirstOrDefault(c => c.name == evt.newValue);
                    CanvasFilterId = match.id;
                }
                OnFilterChanged?.Invoke();
            });
        }

        public void PopulateCanvasDropdown(IReadOnlyList<UIElementEntry> entries)
        {
            _canvasOptions.Clear();
            var seen = new HashSet<int>();

            foreach (var e in entries)
            {
                if (seen.Add(e.RootCanvasId))
                    _canvasOptions.Add((e.RootCanvasId, e.RootCanvasName));
            }

            var choices = new List<string> { "All" };
            choices.AddRange(_canvasOptions.Select(c => c.name));
            _canvasDropdown.choices = choices;
            _canvasDropdown.SetValueWithoutNotify("All");
            CanvasFilterId = -1;
        }
    }
}
```

### `Editor/Panels/UIStackListPanel.cs`
```csharp
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDepthInspector.Editor.Panels
{
    using Core;
    using Diagnostics;

    public class UIStackListPanel
    {
        public Action<UIElementEntry> OnEntryClicked;
        public Action<UIElementEntry> OnActiveToggled;
        public Action<UIElementEntry> OnRaycastToggled;
        public Action<UIElementEntry> OnSoloToggled;

        ListView _listView;
        Button _copyBtn;
        List<UIElementEntry> _filteredEntries = new();
        IReadOnlyList<UIElementEntry> _allEntries;
        int _selectedGlobalIndex = -1;

        public void Bind(VisualElement root)
        {
            _listView = root.Q<ListView>("stack-list");
            _listView.makeItem = MakeRow;
            _listView.bindItem = BindRow;
            _listView.selectionType = SelectionType.Single;
            _listView.itemsSource = _filteredEntries;
            _listView.fixedItemHeight = 26;

            _listView.selectionChanged += selection =>
            {
                foreach (var item in selection)
                {
                    if (item is UIElementEntry entry)
                        OnEntryClicked?.Invoke(entry);
                }
            };

            _copyBtn = root.Q<Button>("btn-copy-stack");
            _copyBtn.clicked += CopyStackToClipboard;
        }

        public void SetEntries(IReadOnlyList<UIElementEntry> entries, UIToolbarPanel toolbar)
        {
            _allEntries = entries;
            ApplyFilters(toolbar);
        }

        public void ApplyFilters(UIToolbarPanel toolbar)
        {
            _filteredEntries.Clear();

            if (_allEntries == null) return;

            foreach (var e in _allEntries)
            {
                if (toolbar.ActiveOnly && !e.IsActive) continue;
                if (toolbar.RaycastOnly && !e.RaycastTarget) continue;
                if (toolbar.WarningsOnly && (e.Flags & DiagnosticFlags.GhostBlocker) == 0) continue;
                if (toolbar.CanvasFilterId >= 0 && e.RootCanvasId != toolbar.CanvasFilterId) continue;
                if (!string.IsNullOrEmpty(toolbar.SearchQuery) &&
                    e.Name.IndexOf(toolbar.SearchQuery, StringComparison.OrdinalIgnoreCase) < 0) continue;

                _filteredEntries.Add(e);
            }

            _listView.itemsSource = _filteredEntries;
            _listView.Rebuild();
        }

        public void SelectEntry(int globalDrawIndex)
        {
            _selectedGlobalIndex = globalDrawIndex;
            for (int i = 0; i < _filteredEntries.Count; i++)
            {
                if (_filteredEntries[i].GlobalDrawIndex == globalDrawIndex)
                {
                    _listView.SetSelectionWithoutNotify(new[] { i });
                    _listView.ScrollToItem(i);
                    return;
                }
            }
        }

        VisualElement MakeRow()
        {
            var row = new VisualElement();
            row.AddToClassList("stack-row");

            row.Add(new Label { name = "index", pickingMode = PickingMode.Ignore });
            row.Add(new VisualElement { name = "dot" });
            row.Add(new Label { name = "name", pickingMode = PickingMode.Ignore });
            row.Add(new Label { name = "warning", pickingMode = PickingMode.Ignore });

            var eyeBtn = new Button { name = "eye", text = "👁" };
            eyeBtn.AddToClassList("row-btn");
            row.Add(eyeBtn);

            var rayBtn = new Button { name = "ray", text = "◎" };
            rayBtn.AddToClassList("row-btn");
            row.Add(rayBtn);

            var soloBtn = new Button { name = "solo", text = "S" };
            soloBtn.AddToClassList("row-btn");
            row.Add(soloBtn);

            return row;
        }

        void BindRow(VisualElement row, int index)
        {
            if (index < 0 || index >= _filteredEntries.Count) return;
            var entry = _filteredEntries[index];

            var indexLabel = row.Q<Label>("index");
            indexLabel.AddToClassList("stack-index");
            indexLabel.text = $"#{entry.GlobalDrawIndex:D2}";

            // Replace dot
            var oldDot = row.Q("dot");
            var newDot = UIDiagnosticBadges.CreateDot(entry.Flags);
            newDot.name = "dot";
            row.Insert(row.IndexOf(oldDot), newDot);
            oldDot.RemoveFromHierarchy();

            var nameLabel = row.Q<Label>("name");
            nameLabel.AddToClassList("stack-name");
            nameLabel.text = entry.Name;
            nameLabel.EnableInClassList("stack-name--inactive", !entry.IsActive);

            var warningLabel = row.Q<Label>("warning");
            warningLabel.AddToClassList("warning-badge");
            bool isGhost = (entry.Flags & DiagnosticFlags.GhostBlocker) != 0;
            warningLabel.text = isGhost ? "⚠ Invisible Hitbox" : "";
            warningLabel.style.display = isGhost ? DisplayStyle.Flex : DisplayStyle.None;

            // Wire button callbacks
            var eyeBtn = row.Q<Button>("eye");
            eyeBtn.clickable = new Clickable(() => OnActiveToggled?.Invoke(entry));

            var rayBtn = row.Q<Button>("ray");
            rayBtn.clickable = new Clickable(() => OnRaycastToggled?.Invoke(entry));

            var soloBtn = row.Q<Button>("solo");
            soloBtn.clickable = new Clickable(() => OnSoloToggled?.Invoke(entry));
        }

        void CopyStackToClipboard()
        {
            if (_filteredEntries.Count == 0) return;

            var sb = new System.Text.StringBuilder();
            string canvasName = _filteredEntries[0].RootCanvasName;
            sb.AppendLine($"UI Depth Inspector — {canvasName} — {_filteredEntries.Count} elements");

            foreach (var e in _filteredEntries)
            {
                string raycast = e.RaycastTarget ? "ON" : "OFF";
                string line = $"#{e.GlobalDrawIndex:D2}  {e.Name,-24} Raycast:{raycast}  Alpha:{e.EffectiveAlpha:F1}";

                if ((e.Flags & DiagnosticFlags.GhostBlocker) != 0)
                    line += "  ⚠ INVISIBLE HITBOX";

                sb.AppendLine(line);
            }

            EditorGUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log("Stack copied to clipboard.");
        }
    }
}
```

## Instructions
1. Create directories `Editor/Resources` and `Editor/Panels` if they do not exist.
2. Create all 5 specified files with their exact contents verbatim.
3. Stage with `git add Editor/Resources/UIDepthInspector.uxml Editor/Resources/UIDepthInspector.uss Editor/Diagnostics/UIDiagnosticBadges.cs Editor/Panels/UIToolbarPanel.cs Editor/Panels/UIStackListPanel.cs`.
4. Commit with message: `feat(panels): add UXML/USS layout, toolbar, and stack list panels`.
5. Write report to `.superpowers/sdd/task-8-report.md`.
