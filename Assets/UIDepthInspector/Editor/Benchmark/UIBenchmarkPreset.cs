using System;

namespace UIDepthInspector.Editor.Benchmark
{
    public enum UIBenchmarkPresetType
    {
        CleanReference,
        CasualHud,
        DeepProduction,
        ChaoticStress,
        Custom
    }

    [Serializable]
    public class UIBenchmarkPresetConfig
    {
        public UIBenchmarkPresetType presetType;
        public string name;
        public int targetElementCount;
        public int canvasCount;
        public bool injectGhostBlockers;
        public bool injectSpatialOverlaps;
        public bool injectMissingSprites;
        public bool injectNestedLabelRaycasts;
        public bool injectCanvasGroupTraps;
    }

    public static class UIBenchmarkPreset
    {
        public static UIBenchmarkPresetConfig GetConfig(UIBenchmarkPresetType type)
        {
            switch (type)
            {
                case UIBenchmarkPresetType.CleanReference:
                    return new UIBenchmarkPresetConfig
                    {
                        presetType = type,
                        name = "Clean Reference",
                        targetElementCount = 20,
                        canvasCount = 1,
                        injectGhostBlockers = false,
                        injectSpatialOverlaps = false,
                        injectMissingSprites = false,
                        injectNestedLabelRaycasts = false,
                        injectCanvasGroupTraps = false
                    };
                case UIBenchmarkPresetType.CasualHud:
                    return new UIBenchmarkPresetConfig
                    {
                        presetType = type,
                        name = "Casual HUD & Popups",
                        targetElementCount = 35,
                        canvasCount = 1,
                        injectGhostBlockers = true,
                        injectSpatialOverlaps = true,
                        injectMissingSprites = false,
                        injectNestedLabelRaycasts = true,
                        injectCanvasGroupTraps = false
                    };
                case UIBenchmarkPresetType.DeepProduction:
                    return new UIBenchmarkPresetConfig
                    {
                        presetType = type,
                        name = "Deep Production UI",
                        targetElementCount = 60,
                        canvasCount = 2,
                        injectGhostBlockers = false,
                        injectSpatialOverlaps = false,
                        injectMissingSprites = true,
                        injectNestedLabelRaycasts = true,
                        injectCanvasGroupTraps = true
                    };
                case UIBenchmarkPresetType.ChaoticStress:
                    return new UIBenchmarkPresetConfig
                    {
                        presetType = type,
                        name = "Chaotic Stress Test",
                        targetElementCount = 80,
                        canvasCount = 2,
                        injectGhostBlockers = true,
                        injectSpatialOverlaps = true,
                        injectMissingSprites = true,
                        injectNestedLabelRaycasts = true,
                        injectCanvasGroupTraps = true
                    };
                case UIBenchmarkPresetType.Custom:
                default:
                    return new UIBenchmarkPresetConfig
                    {
                        presetType = type,
                        name = "Custom Benchmark",
                        targetElementCount = 40,
                        canvasCount = 1,
                        injectGhostBlockers = false,
                        injectSpatialOverlaps = false,
                        injectMissingSprites = false,
                        injectNestedLabelRaycasts = false,
                        injectCanvasGroupTraps = false
                    };
            }
        }
    }
}
