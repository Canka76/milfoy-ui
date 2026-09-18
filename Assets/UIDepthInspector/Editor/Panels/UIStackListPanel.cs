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
            if (_listView != null)
            {
                _listView.makeItem = MakeRow;
                _listView.bindItem = BindRow;
                _listView.selectionType = SelectionType.Single;
                _listView.itemsSource = _filteredEntries;
                _listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                _listView.style.flexGrow = 1;
                _listView.style.flexShrink = 0;
                _listView.style.minHeight = 100;
            }
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
            var rowContainer = new VisualElement { name = "row-container" };
            rowContainer.AddToClassList("stack-row-container");
            rowContainer.style.flexDirection = FlexDirection.Column;
            rowContainer.style.borderBottomWidth = 1;
            rowContainer.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            rowContainer.style.paddingTop = 2;
            rowContainer.style.paddingBottom = 2;

            // Main Row (Horizontal)
            var mainRow = new VisualElement { name = "main-row" };
            mainRow.AddToClassList("stack-row");
            mainRow.style.flexDirection = FlexDirection.Row;
            mainRow.style.alignItems = Align.Center;
            mainRow.style.height = 24;
            mainRow.style.paddingLeft = 4;
            mainRow.style.paddingRight = 4;

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
            mainRow.Add(indexLbl);

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
            mainRow.Add(dot);

            var nameLbl = new Label { name = "name", pickingMode = PickingMode.Ignore };
            nameLbl.AddToClassList("stack-name");
            nameLbl.style.flexGrow = 1;
            nameLbl.style.flexShrink = 1;
            nameLbl.style.overflow = Overflow.Hidden;
            nameLbl.style.unityTextAlign = TextAnchor.MiddleLeft;
            nameLbl.style.fontSize = 12;
            nameLbl.style.marginRight = 4;
            mainRow.Add(nameLbl);

            var warningBadge = new Label { name = "warning-badge", text = "⚠ WARNING", pickingMode = PickingMode.Ignore };
            warningBadge.style.flexShrink = 0;
            warningBadge.style.fontSize = 9;
            warningBadge.style.color = new Color(1f, 0.72f, 0.2f, 1f);
            warningBadge.style.backgroundColor = new Color(1f, 0.72f, 0.2f, 0.15f);
            warningBadge.style.borderTopLeftRadius = 3;
            warningBadge.style.borderTopRightRadius = 3;
            warningBadge.style.borderBottomLeftRadius = 3;
            warningBadge.style.borderBottomRightRadius = 3;
            warningBadge.style.paddingLeft = 4;
            warningBadge.style.paddingRight = 4;
            warningBadge.style.paddingTop = 1;
            warningBadge.style.paddingBottom = 1;
            warningBadge.style.marginRight = 6;
            warningBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
            mainRow.Add(warningBadge);

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
            actions.Add(eyeBtn);

            var rayBtn = new Button { name = "ray", text = "◎", tooltip = "Toggle Graphic.raycastTarget (Turn OFF to stop blocking clicks on objects below)" };
            rayBtn.AddToClassList("row-btn");
            rayBtn.style.width = 22;
            rayBtn.style.height = 22;
            rayBtn.style.minWidth = 22;
            rayBtn.style.minHeight = 22;
            rayBtn.style.flexShrink = 0;
            rayBtn.style.marginLeft = 2;
            actions.Add(rayBtn);

            var soloBtn = new Button { name = "solo", text = "S", tooltip = "Solo Isolate (Hide siblings under this Canvas to inspect alone. Press ~ to restore)" };
            soloBtn.AddToClassList("row-btn");
            soloBtn.style.width = 22;
            soloBtn.style.height = 22;
            soloBtn.style.minWidth = 22;
            soloBtn.style.minHeight = 22;
            soloBtn.style.flexShrink = 0;
            soloBtn.style.marginLeft = 2;
            actions.Add(soloBtn);

            mainRow.Add(actions);
            rowContainer.Add(mainRow);

            // Sub-row: Inline Diagnostic Reason Log
            var logBox = new VisualElement { name = "inline-log-box" };
            logBox.style.marginLeft = 42;
            logBox.style.marginRight = 6;
            logBox.style.marginTop = 2;
            logBox.style.marginBottom = 3;
            logBox.style.paddingLeft = 6;
            logBox.style.paddingRight = 6;
            logBox.style.paddingTop = 3;
            logBox.style.paddingBottom = 3;
            logBox.style.backgroundColor = new Color(0.18f, 0.15f, 0.12f, 0.9f);
            logBox.style.borderLeftWidth = 2;
            logBox.style.borderLeftColor = new Color(1f, 0.72f, 0.2f, 1f);
            logBox.style.borderTopRightRadius = 3;
            logBox.style.borderBottomRightRadius = 3;

            var logLbl = new Label { name = "inline-log-text", pickingMode = PickingMode.Ignore };
            logLbl.style.fontSize = 10;
            logLbl.style.color = new Color(0.95f, 0.78f, 0.45f, 1f);
            logLbl.style.whiteSpace = WhiteSpace.Normal;
            logBox.Add(logLbl);

            rowContainer.Add(logBox);

            return rowContainer;
        }

        void BindRow(VisualElement row, int index)
        {
            if (index < 0 || index >= _filteredEntries.Count) return;
            var entry = _filteredEntries[index];

            var indexLabel = row.Q<Label>("index");
            indexLabel.text = $"#{entry.GlobalDrawIndex:D2}";

            // Update dot in-place
            var dot = row.Q("dot");
            if (dot != null)
                UIDiagnosticBadges.UpdateDot(dot, entry.Flags);

            var nameLabel = row.Q<Label>("name");
            nameLabel.text = entry.Name;
            nameLabel.EnableInClassList("stack-name--inactive", !entry.IsActive);

            // Warning badge & inline log details
            bool isGhost = (entry.Flags & DiagnosticFlags.GhostBlocker) != 0;
            bool isZeroSize = (entry.Flags & DiagnosticFlags.ZeroSize) != 0;
            bool hasWarning = isGhost || isZeroSize;

            var warningBadge = row.Q<Label>("warning-badge");
            if (warningBadge != null)
            {
                warningBadge.style.display = hasWarning ? DisplayStyle.Flex : DisplayStyle.None;
                warningBadge.text = isGhost ? "⚠ INVISIBLE HITBOX" : "ℹ ZERO SIZE";
            }

            var logBox = row.Q("inline-log-box");
            var logLbl = row.Q<Label>("inline-log-text");
            if (logBox != null && logLbl != null)
            {
                if (isGhost)
                {
                    logBox.style.display = DisplayStyle.Flex;
                    logLbl.text = (entry.EffectiveAlpha <= 0f)
                        ? "• Alpha is 0 (invisible) but Raycast Target is enabled — intercepts clicks."
                        : "• Image sprite is missing (null) while Raycast Target is enabled.";
                }
                else if (isZeroSize)
                {
                    logBox.style.display = DisplayStyle.Flex;
                    logLbl.text = "• RectTransform width/height is near zero.";
                }
                else
                {
                    logBox.style.display = DisplayStyle.None;
                }
            }

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
