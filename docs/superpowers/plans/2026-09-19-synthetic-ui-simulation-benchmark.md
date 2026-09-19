# Synthetic UI Simulation & Dual-Agent Benchmark Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a procedural uGUI synthetic scene generator, ground-truth manifest exporter, in-editor Benchmark Lab window, batchmode CLI runner, and dual-agent performance evaluation suite comparing Milfoy-equipped agents against baseline agents across 5 UI anomaly categories.

**Architecture:** A deterministic `UISyntheticSceneGenerator` builds randomized uGUI hierarchies with seeded anomaly injections. A serializable `UIBenchmarkGroundTruth` logs exact coordinates, hierarchy paths, and expected fixes. `UIBenchmarkWindow` (accessible via `Tools > Milfoy > Benchmark Lab`) provides an in-editor GUI for visual generation and 3D preview inspection. `UIDepthInspectorCLI` adds headless batchmode entrypoints. `UIBenchmarkEvaluator` compares reported agent findings against ground-truth JSON to calculate Tokens, Turns, Latency, Precision, Recall, and F1 score, outputting formatted Markdown and CSV scoreboards.

**Tech Stack:** Unity 2022.3+ / 6000+, C#, Unity uGUI, UnityEditor, NUnit / Custom Test Runner.

## Global Constraints
- Zero heap allocation in hot execution loops; zero background idle drag.
- All synthetic GameObjects must be parented under a root `[Benchmark_Generated_UI]` container and support clean, leak-free teardown (`Object.DestroyImmediate`).
- Deterministic reproducibility: same seed + preset must generate the exact same hierarchy and anomaly positions.
- Unit tests must be added to `AutoTestRunner.cs` and `UIBenchmarkTests.cs`.

---

## File Structure & Responsibilities

```
Assets/UIDepthInspector/Editor/
├── Benchmark/
│   ├── UIBenchmarkGroundTruth.cs       # Data models for injected anomalies and ground-truth JSON serialization
│   ├── UISyntheticSceneGenerator.cs   # Procedural builder for uGUI elements and 5 anomaly injections
│   ├── UIBenchmarkPreset.cs           # Preset configurations (Clean, Casual, Deep, Chaos)
│   ├── UIBenchmarkEvaluator.cs        # Metric evaluator (TP/FP/FN, precision, recall, F1, token ratios)
│   └── UIBenchmarkWindow.cs           # EditorWindow with 3D preview integration and packaging tools
├── Export/
│   └── UIDepthInspectorCLI.cs         # Batchmode CLI extension for -executeMethod RunBenchmark
Assets/UIDepthInspector/Tests/Editor/
├── UIBenchmarkTests.cs                # NUnit & AutoTestRunner test cases for generator, ground truth, and evaluator
└── AutoTestRunner.cs                  # Registered test runner suite
```

---

### Task 1: Ground Truth Data Models & Serialization

**Files:**
- Create: `Assets/UIDepthInspector/Editor/Benchmark/UIBenchmarkGroundTruth.cs`
- Create: `Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs`
- Modify: `Assets/UIDepthInspector/Tests/Editor/AutoTestRunner.cs`

**Interfaces:**
- Produces: `UIBenchmarkGroundTruth`, `InjectedAnomalyEntry`, `UIBenchmarkResult`, `UIBenchmarkGroundTruth.ToJson(UIBenchmarkGroundTruth, bool)`, `UIBenchmarkGroundTruth.FromJson(string)`

- [ ] **Step 1: Write unit tests for ground truth serialization**

In `Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs`:
```csharp
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UIDepthInspector.Editor.Benchmark;

namespace UIDepthInspector.Editor.Tests
{
    public class UIBenchmarkTests
    {
        [Test]
        public void GroundTruth_Serialization_RoundTripsAccurately()
        {
            var gt = new UIBenchmarkGroundTruth
            {
                benchmarkId = "BENCH_TEST_001",
                seed = 42,
                preset = "CasualHud",
                totalElements = 25,
                totalAnomalies = 2
            };

            gt.anomalies.Add(new InjectedAnomalyEntry
            {
                anomalyId = "ANO_001",
                type = "ANOMALY_GHOST_BLOCKER",
                targetPath = "Canvas/Panel/GhostBackdrop",
                component = "Image",
                property = "raycastTarget",
                injectedValue = "true",
                expectedValue = "false",
                affectedTarget = "Canvas/Panel/Button_Play"
            });

            string json = UIBenchmarkGroundTruth.ToJson(gt, prettyPrint: true);
            Assert.IsNotEmpty(json);

            var loaded = UIBenchmarkGroundTruth.FromJson(json);
            Assert.AreEqual("BENCH_TEST_001", loaded.benchmarkId);
            Assert.AreEqual(42, loaded.seed);
            Assert.AreEqual(1, loaded.anomalies.Count);
            Assert.AreEqual("ANOMALY_GHOST_BLOCKER", loaded.anomalies[0].type);
            Assert.AreEqual("Canvas/Panel/Button_Play", loaded.anomalies[0].affectedTarget);
        }
    }
}
```

- [ ] **Step 2: Implement `UIBenchmarkGroundTruth.cs`**

In `Assets/UIDepthInspector/Editor/Benchmark/UIBenchmarkGroundTruth.cs`:
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIDepthInspector.Editor.Benchmark
{
    [Serializable]
    public class InjectedAnomalyEntry
    {
        public string anomalyId;
        public string type;
        public string targetPath;
        public string component;
        public string property;
        public string injectedValue;
        public string expectedValue;
        public string affectedTarget;
    }

    [Serializable]
    public class UIBenchmarkGroundTruth
    {
        public string benchmarkId;
        public int seed;
        public string preset;
        public string generatedAt;
        public int totalElements;
        public int totalAnomalies;
        public List<InjectedAnomalyEntry> anomalies = new List<InjectedAnomalyEntry>();

        public static string ToJson(UIBenchmarkGroundTruth gt, bool prettyPrint = true)
        {
            return JsonUtility.ToJson(gt, prettyPrint);
        }

        public static UIBenchmarkGroundTruth FromJson(string json)
        {
            return JsonUtility.FromJson<UIBenchmarkGroundTruth>(json);
        }
    }
}
```

- [ ] **Step 3: Register test into `AutoTestRunner.cs` and run test**

Add `GroundTruth_Serialization_RoundTripsAccurately` into `AutoTestRunner.RunAllUnitTests()`.
Verify PASS.

- [ ] **Step 4: Commit**
```bash
git add Assets/UIDepthInspector/Editor/Benchmark/UIBenchmarkGroundTruth.cs Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs Assets/UIDepthInspector/Tests/Editor/AutoTestRunner.cs
git commit -m "feat(benchmark): add UIBenchmarkGroundTruth models and JSON serialization"
```

---

### Task 2: Presets & Procedural Canvas Hierarchy Generator

**Files:**
- Create: `Assets/UIDepthInspector/Editor/Benchmark/UIBenchmarkPreset.cs`
- Create: `Assets/UIDepthInspector/Editor/Benchmark/UISyntheticSceneGenerator.cs`
- Modify: `Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs`
- Modify: `Assets/UIDepthInspector/Tests/Editor/AutoTestRunner.cs`

**Interfaces:**
- Produces: `UIBenchmarkPresetType`, `UIBenchmarkPresetConfig`, `UISyntheticSceneGenerator.Generate(UIBenchmarkPresetConfig, int seed, out UIBenchmarkGroundTruth)`, `UISyntheticSceneGenerator.ClearBenchmarkUI()`

- [ ] **Step 1: Write test for deterministic generation and cleanup**

In `Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs`:
```csharp
[Test]
public void Generator_CleanPreset_ProducesHierarchyWithZeroAnomalies()
{
    var config = UIBenchmarkPreset.GetConfig(UIBenchmarkPresetType.CleanReference);
    var root = UISyntheticSceneGenerator.Generate(config, seed: 100, out var groundTruth);
    try
    {
        Assert.IsNotNull(root);
        Assert.AreEqual(0, groundTruth.totalAnomalies);
        Assert.AreEqual(0, groundTruth.anomalies.Count);
        Assert.Greater(groundTruth.totalElements, 10);
    }
    finally
    {
        UISyntheticSceneGenerator.ClearBenchmarkUI();
        Assert.IsNull(GameObject.Find(UISyntheticSceneGenerator.RootContainerName));
    }
}
```

- [ ] **Step 2: Implement `UIBenchmarkPreset.cs`**

```csharp
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

    [System.Serializable]
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
                    return new UIBenchmarkPresetConfig { presetType = type, name = "Clean Reference", targetElementCount = 20, canvasCount = 1 };
                case UIBenchmarkPresetType.CasualHud:
                    return new UIBenchmarkPresetConfig { presetType = type, name = "Casual HUD & Popups", targetElementCount = 35, canvasCount = 1, injectGhostBlockers = true, injectSpatialOverlaps = true, injectNestedLabelRaycasts = true };
                case UIBenchmarkPresetType.DeepProduction:
                    return new UIBenchmarkPresetConfig { presetType = type, name = "Deep Production UI", targetElementCount = 60, canvasCount = 2, injectCanvasGroupTraps = true, injectNestedLabelRaycasts = true, injectMissingSprites = true };
                case UIBenchmarkPresetType.ChaoticStress:
                default:
                    return new UIBenchmarkPresetConfig { presetType = type, name = "Chaotic Stress Test", targetElementCount = 80, canvasCount = 2, injectGhostBlockers = true, injectSpatialOverlaps = true, injectMissingSprites = true, injectNestedLabelRaycasts = true, injectCanvasGroupTraps = true };
            }
        }
    }
}
```

- [ ] **Step 3: Implement `UISyntheticSceneGenerator.cs`**

Implements programmatic Canvas creation, standard containers (Panels, Buttons with Text, Images, LayoutGroups), anomaly injection logic for all 5 categories, and ground truth tracking.
Ensures root naming `[Benchmark_Generated_UI]` and clean teardown via `ClearBenchmarkUI()`.

- [ ] **Step 4: Run tests and verify**
Run `AutoTestRunner.RunAllUnitTests()`. Verify PASS.

- [ ] **Step 5: Commit**
```bash
git add Assets/UIDepthInspector/Editor/Benchmark/UIBenchmarkPreset.cs Assets/UIDepthInspector/Editor/Benchmark/UISyntheticSceneGenerator.cs Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs Assets/UIDepthInspector/Tests/Editor/AutoTestRunner.cs
git commit -m "feat(benchmark): add preset configs and UISyntheticSceneGenerator with anomaly injection"
```

---

### Task 3: Anomaly Injection Engine & Ground Truth Verification Tests

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Benchmark/UISyntheticSceneGenerator.cs`
- Modify: `Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs`
- Modify: `Assets/UIDepthInspector/Tests/Editor/AutoTestRunner.cs`

**Interfaces:**
- Tests and verifies all 5 anomaly categories (`ANOMALY_GHOST_BLOCKER`, `ANOMALY_SPATIAL_OVERLAP`, `ANOMALY_MISSING_SPRITE`, `ANOMALY_NESTED_LABEL_RAYCAST`, `ANOMALY_CANVASGROUP_TRAP`) against Milfoy's `UIDiagnosticAnalyzer`.

- [ ] **Step 1: Write tests for all 5 anomaly categories**

In `Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs`:
```csharp
[Test]
public void Generator_ChaoticPreset_InjectsAllConfiguredAnomalyTypes()
{
    var config = UIBenchmarkPreset.GetConfig(UIBenchmarkPresetType.ChaoticStress);
    var root = UISyntheticSceneGenerator.Generate(config, seed: 400, out var groundTruth);
    try
    {
        Assert.Greater(groundTruth.totalAnomalies, 4);
        var types = new HashSet<string>();
        foreach (var a in groundTruth.anomalies)
            types.Add(a.type);

        Assert.IsTrue(types.Contains("ANOMALY_GHOST_BLOCKER"));
        Assert.IsTrue(types.Contains("ANOMALY_SPATIAL_OVERLAP"));
        Assert.IsTrue(types.Contains("ANOMALY_NESTED_LABEL_RAYCAST"));
    }
    finally
    {
        UISyntheticSceneGenerator.ClearBenchmarkUI();
    }
}
```

- [ ] **Step 2: Connect with Milfoy Diagnostic Analyzer to verify 100% anomaly detection**

Add test `Milfoy_Detects_All_Injected_Benchmark_Anomalies()`:
Generates chaotic scene, collects render tree via `UIRenderTreeCollector.Collect()`, analyzes with `UIDiagnosticAnalyzer.Analyze()`, and verifies that Milfoy flags match ground truth injected targets.

- [ ] **Step 3: Run tests and verify**
Run `AutoTestRunner.RunAllUnitTests()`. Verify PASS.

- [ ] **Step 4: Commit**
```bash
git add Assets/UIDepthInspector/Editor/Benchmark/UISyntheticSceneGenerator.cs Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs Assets/UIDepthInspector/Tests/Editor/AutoTestRunner.cs
git commit -m "feat(benchmark): verify all 5 anomaly types in generator and diagnostic analyzer"
```

---

### Task 4: Metric Evaluator & Comparative Scoreboard Generator

**Files:**
- Create: `Assets/UIDepthInspector/Editor/Benchmark/UIBenchmarkEvaluator.cs`
- Modify: `Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs`
- Modify: `Assets/UIDepthInspector/Tests/Editor/AutoTestRunner.cs`

**Interfaces:**
- Produces: `AgentTrialRecord`, `TrialComparisonMetrics`, `UIBenchmarkEvaluator.Evaluate(UIBenchmarkGroundTruth, AgentTrialRecord, AgentTrialRecord)`, `UIBenchmarkEvaluator.FormatMarkdownReport(TrialComparisonMetrics)`, `UIBenchmarkEvaluator.FormatCsvReport(TrialComparisonMetrics)`

- [ ] **Step 1: Write unit tests for evaluator metrics**

In `Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs`:
```csharp
[Test]
public void Evaluator_CalculatesPrecisionRecallAndF1Accurately()
{
    var gt = new UIBenchmarkGroundTruth { benchmarkId = "B1" };
    gt.anomalies.Add(new InjectedAnomalyEntry { anomalyId = "A1", targetPath = "Canvas/P1", type = "ANOMALY_GHOST_BLOCKER" });
    gt.anomalies.Add(new InjectedAnomalyEntry { anomalyId = "A2", targetPath = "Canvas/P2", type = "ANOMALY_NESTED_LABEL_RAYCAST" });

    var milfoyAgent = new AgentTrialRecord
    {
        agentName = "MilfoyAgent",
        totalPromptTokens = 800,
        totalCompletionTokens = 200,
        turns = 2,
        durationSeconds = 4.5f,
        reportedIssuePaths = new List<string> { "Canvas/P1", "Canvas/P2" }
    };

    var baselineAgent = new AgentTrialRecord
    {
        agentName = "BaselineAgent",
        totalPromptTokens = 12000,
        totalCompletionTokens = 3500,
        turns = 14,
        durationSeconds = 85.0f,
        reportedIssuePaths = new List<string> { "Canvas/P1", "Canvas/HallucinatedElement" }
    };

    var comparison = UIBenchmarkEvaluator.Evaluate(gt, milfoyAgent, baselineAgent);
    Assert.AreEqual(1.0f, comparison.milfoyMetrics.precision, 0.01f);
    Assert.AreEqual(1.0f, comparison.milfoyMetrics.recall, 0.01f);
    Assert.AreEqual(0.5f, comparison.baselineMetrics.precision, 0.01f);
    Assert.AreEqual(0.5f, comparison.baselineMetrics.recall, 0.01f);
    Assert.Greater(comparison.tokenReductionPercentage, 90f);
}
```

- [ ] **Step 2: Implement `UIBenchmarkEvaluator.cs`**

Implements TP/FP/FN scoring, token ratio calculations, and generation of `benchmark-report.md` and `benchmark-report.csv`.

- [ ] **Step 3: Run tests and verify**
Run `AutoTestRunner.RunAllUnitTests()`. Verify PASS.

- [ ] **Step 4: Commit**
```bash
git add Assets/UIDepthInspector/Editor/Benchmark/UIBenchmarkEvaluator.cs Assets/UIDepthInspector/Tests/Editor/UIBenchmarkTests.cs Assets/UIDepthInspector/Tests/Editor/AutoTestRunner.cs
git commit -m "feat(benchmark): implement UIBenchmarkEvaluator and report formatting"
```

---

### Task 5: In-Editor Benchmark Lab Window & 3D Viewport Inspection

**Files:**
- Create: `Assets/UIDepthInspector/Editor/Benchmark/UIBenchmarkWindow.cs`
- Modify: `Assets/UIDepthInspector/Editor/Panels/UIToolbarPanel.cs`

**Interfaces:**
- Adds MenuItem `Tools/Milfoy/Benchmark Lab` opening `UIBenchmarkWindow`.
- Provides preset selection, seed randomizer, 5 anomaly checkboxes, 1-click generation, "Inspect in 3D Viewport", and "Export Agent Challenge Pack" writing trial directories.

- [ ] **Step 1: Implement `UIBenchmarkWindow.cs`**
Uses standard EditorGUI / UI Toolkit to provide intuitive control over preset, seed, anomaly toggles, and direct inspection in Milfoy viewport.

- [ ] **Step 2: Add quick access button in `UIToolbarPanel.cs`**
Add compact "Benchmark" toolbar button next to Export button for easy access.

- [ ] **Step 3: Verify Editor Window compilation and behavior**
Verify window opens cleanly without exceptions.

- [ ] **Step 4: Commit**
```bash
git add Assets/UIDepthInspector/Editor/Benchmark/UIBenchmarkWindow.cs Assets/UIDepthInspector/Editor/Panels/UIToolbarPanel.cs
git commit -m "feat(benchmark): add UIBenchmarkWindow Editor GUI and toolbar integration"
```

---

### Task 6: Headless CLI Runner & Automated Dual-Agent Trial Runner Script

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Export/UIDepthInspectorCLI.cs`
- Create: `tools/run_benchmark_trial.ps1` (or bash/python runner)
- Modify: `README.md` and `AGENTS.md`

**Interfaces:**
- Adds `-executeMethod UIDepthInspector.Editor.Export.UIDepthInspectorCLI.GenerateBenchmark`
- Adds `-executeMethod UIDepthInspector.Editor.Export.UIDepthInspectorCLI.EvaluateBenchmark`

- [ ] **Step 1: Extend `UIDepthInspectorCLI.cs` with benchmark methods**
Parses `-benchmarkPreset`, `-benchmarkSeed`, `-benchmarkOutDir`.
Generates synthetic scene, saves `benchmark-ground-truth.json`, writes `.milfoy/ui-context.md`, and exits.

- [ ] **Step 2: Create trial runner script (`tools/run_benchmark_trial.ps1`)**
Orchestrates automated trials, records execution metrics, and outputs `benchmark-report.md`.

- [ ] **Step 3: Update `AGENTS.md` with Benchmark Documentation**
Documents how AI agents or human engineers can run synthetic benchmarks and evaluate token efficiency.

- [ ] **Step 4: Commit**
```bash
git add Assets/UIDepthInspector/Editor/Export/UIDepthInspectorCLI.cs tools/run_benchmark_trial.ps1 README.md AGENTS.md
git commit -m "feat(benchmark): add CLI entrypoints, trial runner script, and documentation"
```
