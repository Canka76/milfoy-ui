# AGENTS.md — Milfoy uGUI 3D Exploded Viewport & Diagnostic Engine

Welcome, AI agent or human engineer! This repository contains **Milfoy** (`com.openupm.ui-depth-inspector`), a zero-GC 3D depth inspector and AI diagnostic engine for Unity uGUI.

---

## ⚡ CRITICAL FOR AI AGENTS: How to Query Milfoy (Instant & Zero-Lock)

**DO NOT** manually parse `.unity` scene YAML files to compute `RectTransform` bounds, Canvas sorting order overrides, or raycast blockers.
**DO NOT** launch `Unity.exe -batchmode` if Unity is already running (it will fail with a process lock error).

Instead, leverage Milfoy's zero-overhead diagnostic cache:

### Primary Method: Passive Diagnostic Cache (Instant File Read)
Whenever a developer or AI agent saves a scene in the Unity Editor, Milfoy automatically analyzes the Canvas hierarchy and exports token-optimized diagnostics:

- **`.milfoy/ui-context.md`**: Compact diagnostic Markdown summarizing ghost blockers, overlapping spatial occlusions, and redundant raycasts ($\le 300$ tokens).
- **`.milfoy/ui-context.json`**: Structured JSON containing full element lists and deterministic `fixAction` patch payloads for autonomous code repairs.

**Zero Overhead Guarantee**:
- **0% Idle Overhead**: No background threads, no HTTP servers, no open ports, and no polling loops.
- **Pure Event-Driven**: Updates occur strictly when scenes are saved via `EditorSceneManager.sceneSaved`.

Simply read `.milfoy/ui-context.md` (or `.milfoy/ui-context.json`) directly for instant UI intelligence!

---

### Fallback Method: Headless Batchmode CLI (Only in CI/CD without active Editor)
If Unity is completely closed and running in automated CI pipelines:
```bash
Unity.exe -batchmode -quit -projectPath . \
  -executeMethod UIDepthInspector.Editor.Export.UIDepthInspectorCLI.DumpContext \
  -dumpFormat md \
  -dumpMode anomalies \
  -dumpOutput "ui-context.md"
```
---

## 🏗️ Architecture & Core Rules
1. **Zero-GC Guarantees**: Any additions to `UIRenderTreeCollector` or `UIDiagnosticAnalyzer` must avoid heap allocations in hot inspection loops. Always use `TryGetComponent<T>(out var comp)`.
2. **Spatial Occlusion Pre-Computation**: Milfoy computes 2D screen-space touch intersections between overlapping Canvases and buttons directly in C#, tagging occluders with `DiagnosticFlags.OcclusionBlocker` and providing exact pixel overlaps.
3. **Passive Auto-Export**: `UIAIContextAutoExporter` hooks into scene save events to maintain `.milfoy/ui-context.*` without runtime performance costs.
4. **Unit Tests**: Test additions live in `Assets/UIDepthInspector/Tests/Editor/`.

---

## 🧪 Benchmark System & Dual-Agent Evaluation Trials

Milfoy features an automated **Benchmark Lab** for evaluating the accuracy and token efficiency of AI coding agents against ground-truth uGUI defects.

### 🎯 Synthetic Challenge Generation
Challenges can be generated programmatically in batchmode:
```bash
Unity.exe -batchmode -quit -projectPath . \
  -executeMethod UIDepthInspector.Editor.Export.UIDepthInspectorCLI.GenerateBenchmark \
  -benchmarkPreset CasualHud \
  -benchmarkSeed 42 \
  -benchmarkOutDir "BenchmarkTrials/Run_CasualHud_42"
```

**Supported Presets:**
- `CleanReference`: Pristine reference hierarchy (0 anomalies).
- `CasualHud`: Standard gameplay HUD (~35 elements, 3–5 anomalies).
- `DeepProduction`: Multi-screen production hierarchy (~120 elements, 8–12 anomalies).
- `ChaoticStress`: Stress test with nested canvases (~250 elements, 15+ anomalies).

### 📝 Challenge Pack Structure
The output directory contains:
- `benchmark-ground-truth.json`: Ground truth registry of all injected anomalies.
- `challenge-instructions.md`: Instructions and specification for agents.
- `ui-context.md` / `ui-context.json`: Precomputed Milfoy diagnostic context.
- `milfoy-agent/`: Directory for Milfoy-equipped agent trial record.
- `baseline-agent/`: Directory for baseline agent trial record.

### 🤖 Output Specification for Agents (`AgentTrialRecord`)
Agents submit their diagnostic findings as an `AgentTrialRecord` JSON file:
```json
{
  "agentName": "AgentIdentifier",
  "totalPromptTokens": 1050,
  "totalCompletionTokens": 320,
  "turns": 2,
  "durationSeconds": 4.5,
  "reportedIssuePaths": [
    "Canvas/HUD_Panel/GhostOverlay",
    "Canvas/SettingsMenu/OccludedConfirmButton"
  ]
}
```

### 🔍 How AI Agents Should Solve Challenges
1. **Instant Intelligence**: Do **NOT** parse raw scene YAML files or invoke heavy tool pipelines.
2. **Read Milfoy Context**: Simply read `ui-context.md` or `ui-context.json` located in the challenge folder or `.milfoy/`.
3. **Identify Flagged Paths**: Extract paths flagged with `GhostBlocker`, `OcclusionBlocker`, `NestedLabelRaycast`, or `GroupTransparent`/`GroupBlocked`.
4. **Submit Record**: Save your reported paths to `trial-record.json`.

### ⚖️ Running Dual-Agent Evaluation
Compare two agent trial records against the ground truth to produce Markdown and CSV scoreboards:
```bash
Unity.exe -batchmode -quit -projectPath . \
  -executeMethod UIDepthInspector.Editor.Export.UIDepthInspectorCLI.EvaluateBenchmark \
  -groundTruthPath "BenchmarkTrials/Run_CasualHud_42/benchmark-ground-truth.json" \
  -milfoyResultPath "BenchmarkTrials/Run_CasualHud_42/milfoy-agent/trial-record.json" \
  -baselineResultPath "BenchmarkTrials/Run_CasualHud_42/baseline-agent/trial-record.json" \
  -reportOutDir "BenchmarkTrials/Run_CasualHud_42"
```

### 🚀 Automated PowerShell Trial Runner
The `tools/run_benchmark_trial.ps1` script automates challenge generation, dual-agent execution/simulation, and scoreboard generation in a single command:
```powershell
.\tools\run_benchmark_trial.ps1 -Preset CasualHud -Seed 42
```
