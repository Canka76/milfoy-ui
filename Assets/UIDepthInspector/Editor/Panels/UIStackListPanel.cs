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
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.height = 26;
            row.style.paddingLeft = 4;
            row.style.paddingRight = 4;
            row.style.overflow = Overflow.Hidden;

            var indexLbl = new Label { name = "index", pickingMode = PickingMode.Ignore };
            indexLbl.AddToClassList("stack-index");
            indexLbl.style.width = 30;
            indexLbl.style.minWidth = 30;
            indexLbl.style.flexShrink = 0;
            indexLbl.style.unityTextAlign = TextAnchor.MiddleRight;
            indexLbl.style.marginRight = 6;
            indexLbl.style.fontSize = 10;
            indexLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            indexLbl.style.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            row.Add(indexLbl);

            var dot = new VisualElement { name = "dot" };
            dot.AddToClassList("stack-dot");
            dot.style.width = 10;
            dot.style.height = 10;
            dot.style.minWidth = 10;
            dot.style.minHeight = 10;
            dot.style.flexShrink = 0;
            dot.style.borderTopLeftRadius = 5;
            dot.style.borderTopRightRadius = 5;
            dot.style.borderBottomLeftRadius = 5;
            dot.style.borderBottomRightRadius = 5;
            dot.style.marginRight = 6;
            row.Add(dot);

            var nameLbl = new Label { name = "name", pickingMode = PickingMode.Ignore };
            nameLbl.AddToClassList("stack-name");
            nameLbl.style.flexGrow = 1;
            nameLbl.style.flexShrink = 1;
            nameLbl.style.overflow = Overflow.Hidden;
            nameLbl.style.unityTextAlign = TextAnchor.MiddleLeft;
            nameLbl.style.fontSize = 12;
            nameLbl.style.marginRight = 4;
            row.Add(nameLbl);

            var warningLbl = new Label { name = "warning", pickingMode = PickingMode.Ignore };
            warningLbl.AddToClassList("warning-badge");
            warningLbl.style.flexShrink = 0;
            warningLbl.style.fontSize = 10;
            warningLbl.style.color = new Color(1f, 0.72f, 0.2f, 1f);
            warningLbl.style.backgroundColor = new Color(1f, 0.72f, 0.2f, 0.15f);
            warningLbl.style.borderTopLeftRadius = 3;
            warningLbl.style.borderTopRightRadius = 3;
            warningLbl.style.borderBottomLeftRadius = 3;
            warningLbl.style.borderBottomRightRadius = 3;
            warningLbl.style.paddingLeft = 4;
            warningLbl.style.paddingRight = 4;
            warningLbl.style.paddingTop = 1;
            warningLbl.style.paddingBottom = 1;
            warningLbl.style.marginRight = 6;
            warningLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(warningLbl);

            var actions = new VisualElement { name = "actions" };
            actions.AddToClassList("row-actions");
            actions.style.flexDirection = FlexDirection.Row;
            actions.style.alignItems = Align.Center;
            actions.style.flexShrink = 0;
            actions.style.marginLeft = StyleKeyword.Auto;

            var eyeBtn = new Button { name = "eye", text = "👁", tooltip = "Toggle GameObject Active state (Undo supported)" };
            eyeBtn.AddToClassList("row-btn");
            eyeBtn.style.width = 22;
            eyeBtn.style.height = 22;
            eyeBtn.style.minWidth = 22;
            eyeBtn.style.minHeight = 22;
            eyeBtn.style.flexShrink = 0;
            eyeBtn.style.marginLeft = 2;
            eyeBtn.style.paddingLeft = 0;
            eyeBtn.style.paddingRight = 0;
            eyeBtn.style.paddingTop = 0;
            eyeBtn.style.paddingBottom = 0;
            actions.Add(eyeBtn);

            var rayBtn = new Button { name = "ray", text = "◎", tooltip = "Toggle Graphic.raycastTarget (Turn OFF to stop blocking clicks on objects below)" };
            rayBtn.AddToClassList("row-btn");
            rayBtn.style.width = 22;
            rayBtn.style.height = 22;
            rayBtn.style.minWidth = 22;
            rayBtn.style.minHeight = 22;
            rayBtn.style.flexShrink = 0;
            rayBtn.style.marginLeft = 2;
            rayBtn.style.paddingLeft = 0;
            rayBtn.style.paddingRight = 0;
            rayBtn.style.paddingTop = 0;
            rayBtn.style.paddingBottom = 0;
            actions.Add(rayBtn);

            var soloBtn = new Button { name = "solo", text = "S", tooltip = "Solo Isolate (Hide siblings under this Canvas to inspect alone. Press ~ or Escape to restore)" };
            soloBtn.AddToClassList("row-btn");
            soloBtn.style.width = 22;
            soloBtn.style.height = 22;
            soloBtn.style.minWidth = 22;
            soloBtn.style.minHeight = 22;
            soloBtn.style.flexShrink = 0;
            soloBtn.style.marginLeft = 2;
            soloBtn.style.paddingLeft = 0;
            soloBtn.style.paddingRight = 0;
            soloBtn.style.paddingTop = 0;
            soloBtn.style.paddingBottom = 0;
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
