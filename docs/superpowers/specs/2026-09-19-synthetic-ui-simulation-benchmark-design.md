# Design Spec: Milfoy Synthetic UI Simulation & Dual-Agent Benchmark Suite

**Date**: 2026-09-19  
**Status**: Approved (Pending Implementation Plan)  
**Target Package**: `Assets/UIDepthInspector` (Milfoy)  

---

## 1. Executive Summary & Objectives
Milfoy is a zero-GC, zero-idle-overhead 3D viewport and diagnostic engine for Unity uGUI with built-in AI passive caching (`.milfoy/ui-context.md` and `.milfoy/ui-context.json`).

To demonstrate empirical proof of Milfoy's real-world impact and capabilities, this system implements:
1. **`UISyntheticSceneGenerator`**: A procedural scene generator that constructs realistic, randomized uGUI hierarchies with deterministic seeds and injects ground-truth UI anomalies across 5 distinct categories.
2. **Ground-Truth Metadata System**: Automatically exports exact anomaly coordinates, target hierarchy paths, and expected fixes into `benchmark-ground-truth.json`.
3. **In-Editor Benchmark Lab & CLI**: An Editor window (`Tools > Milfoy > Benchmark Lab`) and batchmode CLI entrypoints (`UIDepthInspectorCLI`) to generate scenes, inspect them in 3D, and package agent challenge trials.
4. **Dual-Agent Trial Harness & Scoring Evaluator**: An automated evaluation harness that runs identical diagnostic challenges with two agent configurations:
   - **Agent A (Milfoy-Equipped)**: Uses Milfoy's passive diagnostic cache.
   - **Agent B (Baseline)**: Uses raw scene files, hierarchy inspection, and code search without Milfoy.
5. **Comparative Scoreboard**: Generates markdown and CSV reports measuring **Tokens Used**, **Execution Time**, **Turn Count**, **Precision**, **Recall**, and **F1 Score**.

---

## 2. Anomaly Injection Taxonomy (5 Categories)

The generator injects 5 realistic UI bugs and tracks them in the ground-truth manifest:

| Category ID | Anomaly Name | Description & Injection Mechanism | Expected Ground Truth Fix |
|-------------|--------------|------------------------------------|---------------------------|
| `ANOMALY_GHOST_BLOCKER` | Ghost Raycast Blocker | Transparent image or zero-alpha element with `raycastTarget = true` hovering over buttons. | Disable `raycastTarget` or set `blocksRaycasts = false`. |
| `ANOMALY_SPATIAL_OVERLAP` | Spatial Overlap & Sorting Conflict | A secondary Canvas or popup panel with conflicting sorting order or screen-space overlay blocking interactive buttons. | Adjust sorting order, reposition rect, or disable raycast target on backdrop. |
| `ANOMALY_MISSING_SPRITE` | Missing Sprite / Zero-Size Rect | `Image` component with `sprite == null` or `RectTransform` sized `(0, 0)` acting as an active raycast target. | Assign sprite, fix dimensions, or remove redundant raycast target. |
| `ANOMALY_NESTED_LABEL_RAYCAST` | Nested Label Raycast Trap | `Text` or `TextMeshProUGUI` child element inside a `Button` with `raycastTarget = true` (redundant event interceptor). | Toggle `raycastTarget = false` on nested label. |
| `ANOMALY_CANVASGROUP_TRAP` | CanvasGroup Transparency Trap | Parent `CanvasGroup` has `alpha = 0f` or `interactable = false`, but `blocksRaycasts = true`, intercepting all child raycasts. | Set `blocksRaycasts = false` when alpha is 0. |

---

## 3. Architecture & Core Components

```
Assets/UIDepthInspector/Editor/
├── Benchmark/
│   ├── UISyntheticSceneGenerator.cs       # Procedural Canvas & Anomaly builder
│   ├── UIBenchmarkPreset.cs               # Presets: Clean, Casual, Deep, Chaos
│   ├── UIBenchmarkGroundTruth.cs          # Serializable ground-truth model & exporter
│   ├── UIBenchmarkWindow.cs               # Editor GUI (Tools > Milfoy > Benchmark Lab)
│   └── UIBenchmarkEvaluator.cs            # Accuracy, Precision/Recall & Scoreboard generator
├── Export/
│   ├── UIDepthInspectorCLI.cs             # Extended batchmode CLI flags for benchmark runs
│   └── UIAIContextExporter.cs             # Existing diagnostic cache exporter
└── Tests/Editor/
    └── UIBenchmarkTests.cs                # Unit tests for generation, injection & evaluation
```

### 3.1. `UISyntheticSceneGenerator`
- **Deterministic Seed**: Uses `System.Random(seed)` so that given the same seed and preset, the exact same hierarchy, positions, and anomaly types are generated every time.
- **Hierarchy Templates**:
  - **Canvas Roots**: Configurable 1 to 3 Canvas roots (`ScreenSpaceOverlay`, `ScreenSpaceCamera`).
  - **Layout Groups**: Random combinations of `HorizontalLayoutGroup`, `VerticalLayoutGroup`, `GridLayoutGroup`, and anchored floating panels.
  - **Interactive Elements**: Buttons, Sliders, ScrollViews, InputFields with standard uGUI hierarchy depth.
- **Anomaly Injection Rate**: Configurable via `UIBenchmarkConfig` (e.g., target 3–10 anomalies per generated scene).
- **Cleanup / Isolation**: Generates into a dedicated GameObject root (`[Benchmark_Generated_UI]`) or temporary `.unity` scene to prevent polluting project work.

### 3.2. Presets
1. **Preset: Clean Reference** (Seed: 100):
   - 25 UI elements, standard nested hierarchy, **0 anomalies**. Used to verify zero false positives.
2. **Preset: Casual HUD & Popups** (Seed: 200):
   - Top HUD bar, bottom action bar, center popup modal.
   - Injected anomalies: 2 Ghost Blockers, 1 Spatial Overlap, 2 Nested Label Raycasts.
3. **Preset: Deep Production UI** (Seed: 300):
   - Nested tab views, inventory grids (50+ elements), scrolling lists.
   - Injected anomalies: 3 CanvasGroup Traps, 4 Nested Label Raycasts, 2 Missing Sprites.
4. **Preset: Chaotic Stress Test** (Seed: 400):
   - Multi-canvas overlay with heavy layering (80+ elements).
   - Injected anomalies: Mix of all 5 categories (10+ anomalies).

### 3.3. Ground Truth Manifest (`benchmark-ground-truth.json`)
```json
{
  "benchmarkId": "RUN_400_CHAOS",
  "seed": 400,
  "preset": "ChaoticStress",
  "generatedAt": "2026-09-19T08:50:00Z",
  "totalElements": 84,
  "totalAnomalies": 11,
  "anomalies": [
    {
      "anomalyId": "ANO_001",
      "type": "ANOMALY_GHOST_BLOCKER",
      "targetPath": "Canvas_Overlay/Modal_Reward/Backdrop_Invisible",
      "component": "Image",
      "property": "raycastTarget",
      "injectedValue": true,
      "expectedValue": false,
      "affectedTarget": "Canvas_Overlay/Modal_Reward/Claim_Button"
    }
  ]
}
```

### 3.4. In-Editor Benchmark Window (`UIBenchmarkWindow`)
Accessible via `Tools > Milfoy > Benchmark Lab`:
- **Controls**:
  - Preset Selector Dropdown.
  - Seed Field & `Randomize Seed` button.
  - 5 Anomaly Toggles (`Ghost Blockers`, `Spatial Overlaps`, `Missing Sprites`, `Nested Raycasts`, `CanvasGroup Traps`).
  - Element Count Slider (15 to 120).
- **Actions**:
  - `Generate Benchmark UI`: Constructs the scene and writes ground truth JSON.
  - `Clear Benchmark UI`: Removes synthetic hierarchy cleanly.
  - `Inspect in Milfoy 3D Viewport`: Auto-opens and focuses Milfoy window on generated UI.
  - `Export Agent Challenge Pack`: Saves trial packages into `BenchmarkTrials/<RunId>/`.
  - `Evaluate Findings JSON`: Allows loading an agent's reported findings and calculates the Precision/Recall/F1 scoreboard instantly.

### 3.5. Headless CLI Integration (`UIDepthInspectorCLI.cs`)
Enables zero-UI execution for CI pipelines:
```bash
Unity.exe -batchmode -quit -projectPath . \
  -executeMethod UIDepthInspector.Editor.Export.UIDepthInspectorCLI.RunBenchmark \
  -benchmarkPreset ChaoticStress \
  -benchmarkSeed 42 \
  -benchmarkOutDir "BenchmarkTrials/Run_42"
```

---

## 4. Evaluation Metrics & Comparative Scoreboard

The `UIBenchmarkEvaluator` compares reported agent findings against ground truth:

### Accuracy Metrics
- **True Positive (TP)**: Injected anomaly reported on matching `targetPath` with matching category.
- **False Positive (FP)**: Agent flags an element as defective when it is valid.
- **False Negative (FN)**: Injected anomaly not detected by agent.
- **Precision**: $\frac{\text{TP}}{\text{TP} + \text{FP}}$
- **Recall**: $\frac{\text{TP}}{\text{TP} + \text{FN}}$
- **F1 Score**: $2 \times \frac{\text{Precision} \times \text{Recall}}{\text{Precision} + \text{Recall}}$

### Efficiency Metrics
- **Total Prompt Tokens**: Input tokens consumed across all turns.
- **Total Completion Tokens**: Output tokens generated.
- **Round-Trip Turns**: Number of conversational/tool steps to solve.
- **Wall-Clock Time**: Seconds from task start to final report.

### Output Report Format (`benchmark-report.md`)
```markdown
# Milfoy Dual-Agent Benchmark Report: Chaotic Stress Test (Seed 42)

| Metric | Baseline Agent (No Milfoy) | Milfoy-Equipped Agent | Efficiency Gain |
| :--- | :--- | :--- | :--- |
| **Total Tokens** | 24,150 tokens | 1,420 tokens | **-94.1%** |
| **Tool / Turn Count** | 18 turns | 2 turns | **-88.9%** |
| **Time to Complete** | 128 seconds | 8.4 seconds | **-93.4%** |
| **Ground Truth Issues** | 11 | 11 | — |
| **True Positives (TP)**| 7 | 11 | +57.1% |
| **False Positives (FP)**| 4 | 0 | 0 FP |
| **False Negatives (FN)**| 4 | 0 | 0 FN |
| **Precision** | 63.6% | 100.0% | **+36.4%** |
| **Recall** | 63.6% | 100.0% | **+36.4%** |
| **F1 Score** | 0.636 | 1.000 | **+57.2%** |
```

---

## 5. Testing & Verification Plan
1. **Procedural Generation Unit Tests** (`UIBenchmarkTests.cs`):
   - Verify deterministic generation across identical seeds.
   - Verify that each preset produces the exact expected number of injected anomalies.
   - Verify clean hierarchy destruction without orphan GameObjects.
2. **Ground Truth Validation Tests**:
   - Verify that all injected anomalies in the ground-truth JSON match real component states in the scene.
3. **Evaluator Mathematical Correctness Tests**:
   - Mock perfect, partial, and noisy findings JSONs to confirm precision, recall, and F1 calculations match expected formulas.
4. **Milfoy Diagnostic Integration**:
   - Verify that `.milfoy/ui-context.md` and `.milfoy/ui-context.json` successfully detect 100% of the injected anomalies when saved.
