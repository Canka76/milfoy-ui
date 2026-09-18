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
