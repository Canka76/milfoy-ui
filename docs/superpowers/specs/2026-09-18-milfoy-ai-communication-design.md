# Specification: Milfoy AI Communication & Diagnostic Engine

**Date:** 2026-09-18  
**Status:** Approved  
**Topic:** Token-Optimized Spatial Occlusion Detection and Autonomous AI Action Schema in Milfoy UI Depth Inspector

---

## 1. Overview & Objectives

Milfoy's AI integration aims to provide fast, token-efficient, and pinpoint diagnostic context to both interactive LLM chat sessions (e.g. Gemini, Claude, ChatGPT) and autonomous headless CLI agents.

### Core Objectives:
1. **Reduce Context Token Footprint by 75–85%**: Replace full hierarchy dumps with anomaly-focused and pruned diagnostic modes.
2. **Pre-compute Spatial Occlusion in Engine (C#)**: Detect when higher-order Canvases or overlay graphics physically intercept touches over interactive buttons, eliminating LLM arithmetic errors.
3. **Dual-Mode Output Formats**:
   - **Compact Markdown (`--dumpFormat md`)**: Clean, human/LLM readable anomaly summary with exact spatial occlusion triplets.
   - **Machine-Patchable JSON Actions (`--dumpFormat json`)**: Structured JSON schema with deterministic patch instructions for automated agent execution.

---

## 2. Architectural Components

### 2.1 Spatial Occlusion Analyzer (`UIDiagnosticAnalyzer.cs`)
- Identify all interactive UI elements (`Button`, `Toggle`, `InputField`, `ScrollRect`, etc.).
- Compare against all subsequent elements in global render/draw order (accounting for Canvas `m_SortingOrder`, Canvas nesting, and sibling draw indices).
- When a higher element $A$ with active raycasting (`raycastTarget == true` and `effectiveAlpha > 0`) intersects an interactive element $B$'s screen-space `WorldRect`:
  - Tag $A$ with `DiagnosticFlags.OcclusionBlocker`.
  - Record an `OcclusionPair` containing the blocker path, occluded target path, intersection rect, and overlap percentage.

### 2.2 New Diagnostic Flags (`UIDiagnosticBadges.cs` & `UIElementEntry.cs`)
- `DiagnosticFlags.OcclusionBlocker`: Graphic drawing above an interactive element and intercepting its raycasts.
- `DiagnosticFlags.NestedLabelRaycast`: Unnecessary `raycastTarget` on child text labels inside button hierarchies.

### 2.3 Token-Optimized Exporter (`UIAIContextExporter.cs`)
- **Export Modes**:
  - `anomalies`: Outputs only elements with active diagnostic flags and spatial occlusion pairs (omits all healthy nodes).
  - `compact`: Prunes branches with only healthy nodes into `[+ N healthy elements]`.
  - `full`: Complete hierarchy dump.
- **Markdown Format**:
  - Delivers spatial occlusion triplets with clear `[SPATIAL BLOCK]` tags, overlap percentages, and actionable fixes.
- **JSON Format**:
  - Emits structured `anomalies` array with machine-readable `fixAction` metadata (`targetPath`, `component`, `property`, `value`).

### 2.4 Headless CLI Enhancements (`UIDepthInspectorCLI.cs`)
- Add support for `-dumpMode [anomalies|compact|full]` (default: `anomalies`).
- Add support for `-dumpFormat [md|json]` (default: `md`).

---

## 3. Verification & Acceptance Criteria

1. **Spatial Occlusion Detection**:
   - Running diagnosis on `DemoScene.unity` must automatically detect `Facade/BackgroundImage` (Canvas sortingOrder 15) blocking `Buttons/Button` (Canvas sortingOrder 0) with 100% overlap.
2. **Ghost Blocker Detection**:
   - `Icons/Game_Header` must be flagged as `GHOST_BLOCKER` due to missing sprite with active `raycastTarget`.
3. **Token Efficiency**:
   - Anomaly-only export for `DemoScene.unity` must generate $\le 250$ tokens.
4. **Automated Test Suite**:
   - Add unit tests in `UIAIContextExporterTests.cs` verifying spatial occlusion calculations, anomaly-only filtering, and JSON action schema generation.
