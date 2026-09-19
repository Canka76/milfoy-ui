# Changelog

All notable changes to this project will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-09-19

### Added
- **Interactive 3D Exploded Viewport**:
  - Projects flat 2D uGUI Canvases into an interactive 3D layered scene using `PreviewRenderUtility`.
  - Configurable depth separation slider (Z-axis layer spacing).
  - Orbit, pan, and zoom camera controls supporting both perspective and orthographic projections.
  - Solo element isolation mode (`Double Click` / `Space`) with background layer dimming.
  - Visual front and back orientation indicators in 3D viewport space.
- **Real-Time UI Diagnostic Analyzer**:
  - Zero-allocation inspection loop tagging UI defects.
  - Ghost blocker detection for invisible graphics (`alpha == 0` or transparent color) with active `raycastTarget`.
  - Missing sprite detection on `Image` components and zero-size rect detection.
  - Nested label raycast trap detection on child `Text` or `TextMeshProUGUI` inside buttons.
  - `CanvasGroup` transparency and raycast inheritance analysis.
- **Spatial Occlusion Analysis**:
  - C# 2D screen-space touch intersection engine.
  - Detects overlapping sibling layers and multi-Canvas sorting order conflicts.
  - Computes exact pixel overlap metrics for touch-stealing occluders.
- **Passive AI Diagnostic Cache**:
  - Event-driven auto-exporter hooked into `EditorSceneManager.sceneSaved`.
  - Generates token-optimized `.milfoy/ui-context.md` (<300 tokens) and structured `.milfoy/ui-context.json` with deterministic `fixAction` payloads.
  - 0% idle CPU and RAM overhead with zero open ports or background polling.
- **UI Benchmark Lab**:
  - In-Editor window (`Tools > Milfoy > Benchmark Lab`) for deterministic procedural uGUI generation.
  - 5 defect injection categories across 4 complexity presets (`CleanReference`, `CasualHud`, `DeepProduction`, `ChaoticStress`).
  - Automated dual-agent performance evaluation suite (`tools/run_benchmark_trial.ps1`).
  - Empirical verification demonstrating -94.2% token reduction and 100% precision/recall.
- **Visual Enhancements & Customization**:
  - Per-element custom color picker with `SessionState` persistence.
  - Neon corner bracket selection highlighting with dimming for non-selected layers.
  - Bi-directional 3-way selection synchronization between 3D Viewport, Hierarchy, and Draw-Order Stack List.
- **Headless Batchmode CLI**:
  - `UIDepthInspectorCLI.DumpContext` for headless context extraction in CI/CD.
  - `UIDepthInspectorCLI.GenerateBenchmark` and `EvaluateBenchmark` for unattended challenge trials.
- **Comprehensive Test Suite**:
  - 30 automated unit tests registered in `AutoTestRunner.cs` covering render collection, sorting orders, depth placement, anomaly diagnostics, and CLI pipelines.
