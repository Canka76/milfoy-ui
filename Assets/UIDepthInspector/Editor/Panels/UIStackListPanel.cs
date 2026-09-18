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

            var actions = new VisualElement { name = "actions" };
            actions.AddToClassList("row-actions");

            var eyeBtn = new Button { name = "eye", text = "👁" };
            eyeBtn.AddToClassList("row-btn");
            actions.Add(eyeBtn);

            var rayBtn = new Button { name = "ray", text = "◎" };
            rayBtn.AddToClassList("row-btn");
            actions.Add(rayBtn);

            var soloBtn = new Button { name = "solo", text = "S" };
            soloBtn.AddToClassList("row-btn");
            actions.Add(soloBtn);

            row.Add(actions);

            return row;
        }

        void BindRow(VisualElement row, int index)
        {
            if (index < 0 || index >= _filteredEntries.Count) return;
            var entry = _filteredEntries[index];

            var indexLabel = row.Q<Label>("index");
            indexLabel.AddToClassList("stack-index");
            indexLabel.text = $"#{entry.GlobalDrawIndex:D2}";

            // Update dot in-place
            var dot = row.Q("dot");
            if (dot != null)
                UIDiagnosticBadges.UpdateDot(dot, entry.Flags);

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
