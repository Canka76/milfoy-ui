# Specification: Zero-Overhead Passive AI Diagnostic Cache

**Date:** 2026-09-18  
**Status:** Approved  
**Topic:** Eliminating background networking and implementing a zero-overhead, event-driven diagnostic file exporter in Milfoy UI Depth Inspector.

---

## 1. Core Principles & Philosophy
1. **Strict Zero Runtime Overhead**:
   - **0% CPU and 0 bytes RAM when idle.**
   - **Zero background threads, zero open network ports, zero sockets.**
2. **Compact Problem Solver**:
   - Solve UI issues (ghost blockers, occlusions, raycasts) without introducing system-level friction (firewall popups, port collisions, process locks).
3. **Passive Ambient Cache**:
   - Instead of forcing AI agents to launch heavy batchmode processes, pre-compute diagnostic summaries upon scene save and write them to `.milfoy/ui-context.md`.

---

## 2. Architectural Changes

### 2.1 Complete Removal of HTTP/Network Components
- Permanently delete `Assets/UIDepthInspector/Editor/Export/UIAIContextServer.cs`.
- Remove all `HttpListener`, background listening threads, and port 8765 bindings.

### 2.2 Event-Driven Scene-Save Exporter (`UIAIContextAutoExporter.cs`)
- Hook exclusively into `EditorSceneManager.sceneSaved`.
- On scene save:
  1. Collect root Canvases using zero-allocation `UIRenderTreeCollector`.
  2. Run spatial occlusion analysis via `UIDiagnosticAnalyzer`.
  3. Export compact Markdown ($\le 300$ tokens) to `.milfoy/ui-context.md`.
  4. Export machine-patchable JSON to `.milfoy/ui-context.json`.
- When not saving: Does nothing. Zero background updates, zero polling loops, zero timers.

### 2.3 Documentation & Contract Alignment (`AGENTS.md`)
- Remove all references to `curl` or port 8765.
- Establish reading `.milfoy/ui-context.md` as the direct, zero-risk method for visiting AI agents to diagnose UI issues.
