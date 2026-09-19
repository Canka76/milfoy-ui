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
