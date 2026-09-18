using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDepthInspector.Editor.Panels
{
    using Core;
    using Viewport;

    public class UIToolbarPanel
    {
        public Action<ViewPreset> OnViewPresetChanged;
        public Action<float> OnExplosionChanged;
        public Action<float> OnThicknessChanged;
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
            var toolbar = root.Q("toolbar");
            if (toolbar != null)
            {
                toolbar.style.paddingLeft = 6;
                toolbar.style.paddingRight = 6;
                toolbar.style.paddingTop = 5;
                toolbar.style.paddingBottom = 5;
                toolbar.style.backgroundColor = new Color(0.20f, 0.20f, 0.20f, 1f);
                toolbar.style.borderBottomWidth = 1;
                toolbar.style.borderBottomColor = new Color(0.12f, 0.12f, 0.12f, 1f);
            }

            var row1 = root.Q("toolbar-row-presets");
            if (row1 != null)
            {
                row1.style.flexDirection = FlexDirection.Row;
                row1.style.alignItems = Align.Center;
                row1.style.marginBottom = 4;
            }

            var row2 = root.Q("toolbar-row-filters");
            if (row2 != null)
            {
                row2.style.flexDirection = FlexDirection.Row;
                row2.style.alignItems = Align.Center;
            }

            var sep = root.Q("toolbar-separator");
            if (sep != null)
            {
                sep.style.width = 1;
                sep.style.height = 16;
                sep.style.backgroundColor = new Color(0.35f, 0.35f, 0.35f, 1f);
                sep.style.marginLeft = 6;
                sep.style.marginRight = 6;
            }
            var btnFront = root.Q<Button>("btn-front");
            if (btnFront != null)
            {
                btnFront.style.width = 50;
                btnFront.style.marginRight = 2;
                btnFront.clicked += () => OnViewPresetChanged?.Invoke(ViewPreset.Front);
            }

            var btnIso = root.Q<Button>("btn-iso");
            if (btnIso != null)
            {
                btnIso.style.width = 50;
                btnIso.style.marginRight = 2;
                btnIso.clicked += () => OnViewPresetChanged?.Invoke(ViewPreset.Isometric);
            }

            var btnSide = root.Q<Button>("btn-side");
            if (btnSide != null)
            {
                btnSide.style.width = 50;
                btnSide.style.marginRight = 4;
                btnSide.clicked += () => OnViewPresetChanged?.Invoke(ViewPreset.Side);
            }

            _slider = root.Q<Slider>("slider-explosion");
            _floatField = root.Q<FloatField>("field-explosion");

            if (_slider != null && _floatField != null)
            {
                _slider.style.flexGrow = 1;
                _slider.style.minWidth = 60;
                _floatField.style.width = 45;
                _floatField.style.marginLeft = 4;

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
            }

            var thicknessSlider = root.Q<Slider>("slider-thickness");
            var thicknessField = root.Q<FloatField>("field-thickness");

            if (thicknessSlider != null && thicknessField != null)
            {
                thicknessSlider.style.flexGrow = 1;
                thicknessSlider.style.minWidth = 60;
                thicknessField.style.width = 45;
                thicknessField.style.marginLeft = 4;

                thicknessSlider.RegisterValueChangedCallback(evt =>
                {
                    thicknessField.SetValueWithoutNotify(evt.newValue);
                    OnThicknessChanged?.Invoke(evt.newValue);
                });

                thicknessField.RegisterValueChangedCallback(evt =>
                {
                    float clamped = UnityEngine.Mathf.Clamp(evt.newValue, 0.01f, 0.5f);
                    thicknessSlider.SetValueWithoutNotify(clamped);
                    thicknessField.SetValueWithoutNotify(clamped);
                    OnThicknessChanged?.Invoke(clamped);
                });
            }
            var searchField = root.Q<TextField>("search-field");
            if (searchField != null)
            {
                searchField.RegisterValueChangedCallback(evt =>
                {
                    SearchQuery = evt.newValue ?? "";
                    OnFilterChanged?.Invoke();
                });
            }
            var toggleRaycast = root.Q<Toggle>("toggle-raycast");
            if (toggleRaycast != null)
            {
                toggleRaycast.RegisterValueChangedCallback(evt =>
                {
                    RaycastOnly = evt.newValue;
                    OnFilterChanged?.Invoke();
                });
            }

            var toggleWarnings = root.Q<Toggle>("toggle-warnings");
            if (toggleWarnings != null)
            {
                toggleWarnings.RegisterValueChangedCallback(evt =>
                {
                    WarningsOnly = evt.newValue;
                    OnFilterChanged?.Invoke();
                });
            }

            var toggleActive = root.Q<Toggle>("toggle-active");
            if (toggleActive != null)
            {
                toggleActive.RegisterValueChangedCallback(evt =>
                {
                    ActiveOnly = evt.newValue;
                    OnFilterChanged?.Invoke();
                });
            }

            _canvasDropdown = root.Q<DropdownField>("canvas-filter");
            if (_canvasDropdown != null)
            {
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
        }

        public void PopulateCanvasDropdown(IReadOnlyList<UIElementEntry> entries)
        {
            _canvasOptions.Clear();
            if (entries != null)
            {
                var seen = new HashSet<int>();
                foreach (var e in entries)
                {
                    if (!string.IsNullOrEmpty(e.RootCanvasName) && seen.Add(e.RootCanvasId))
                        _canvasOptions.Add((e.RootCanvasId, e.RootCanvasName));
                }
            }

            if (_canvasDropdown != null)
            {
                var choices = new List<string> { "All" };
                choices.AddRange(_canvasOptions.Select(c => c.name));
                _canvasDropdown.choices = choices;
                _canvasDropdown.SetValueWithoutNotify("All");
            }
            CanvasFilterId = -1;
        }
    }
}
