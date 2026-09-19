# Implementation Plan: Making Milfoy the Obvious, Frictionless Choice for AI Agents

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Transform Milfoy from a tool requiring active, risky batchmode invocation into an ambient, zero-friction system that AI agents discover and reach for naturally via passive on-disk artifacts, native MCP tool definitions, and live HTTP IPC.

**Architecture:** 
1. **Passive Ambient Cache:** Automatically maintain `.milfoy/ui-context.md` and `.milfoy/ui-context.json` on scene save, play mode toggle, and editor load so agents find instant answers during routine file exploration.
2. **Local HTTP Daemon:** Keep `UIAIContextServer` running inside the Unity Editor on port `8765` so queries execute in <15ms without process locks.
3. **Model Context Protocol (MCP) Server / Tool Schema:** Provide a standalone, lightweight Node/Python MCP wrapper (`tools/milfoy-mcp/`) that AI environments (Cursor, Windsurf, Claude Desktop, Antigravity) register automatically.
4. **Unity Test Runner Integration:** Inject a diagnostic health check into Unity's automated test runner so anomalies surface automatically when an agent runs project tests.

**Tech Stack:** C# (Unity Editor 6000.0+), Node.js / Python (MCP server wrapper), JSON / Markdown.

## Global Constraints
- Zero heap allocations in C# hot paths (`TryGetComponent`, pooled lists).
- No file lock conflicts with active Unity Editor instances.
- Pre-computed on-disk markdown reports must stay $\le 400$ tokens for optimal agent context economy.

---

### Task 1: Ambient Passive Artifact Generator (`UIAIContextAutoExporter`)

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Export/UIAIContextAutoExporter.cs`
- Modify: `Assets/UIDepthInspector/Editor/UIDepthInspectorWindow.cs`
- Test: `Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs`

**Interfaces:**
- Produces: `.milfoy/ui-context.md` and `.milfoy/ui-context.json` updated automatically on:
  1. `EditorSceneManager.sceneSaved`
  2. `EditorSceneManager.sceneOpened`
  3. `EditorApplication.playModeStateChanged`
  4. Domain reload / project open (`[InitializeOnLoad]`)

- [ ] **Step 1: Write unit test validating auto-exporter writes valid compact anomaly markdown on scene events**
- [ ] **Step 2: Connect all editor lifecycle hooks with debounced writing (avoiding disk churn)**
- [ ] **Step 3: Ensure `.milfoy/` folder and files are git-tracked or clearly discoverable in project root**
- [ ] **Step 4: Verify that running Editor updates `.milfoy/ui-context.md` without any manual commands**

---

### Task 2: Robust In-Editor HTTP Daemon (`UIAIContextServer`)

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Export/UIAIContextServer.cs`
- Test: `Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs`

**Interfaces:**
- Produces: Persistent `http://127.0.0.1:8765/milfoy/diagnostics` endpoint responding in <15ms.
- Parameters: `?format=md|json`, `?mode=anomalies|compact|full`, `?canvas=<name>`.

- [ ] **Step 1: Verify socket lifecycle handling (clean shutdown on domain reload / editor exit)**
- [ ] **Step 2: Implement thread-safe dispatch to Unity's main thread with timeout fallback**
- [ ] **Step 3: Add `/milfoy/status` healthcheck route returning active scene name and canvas count**
- [ ] **Step 4: Test HTTP response latency and verify lock-free execution while Unity is actively rendering**

---

### Task 3: Model Context Protocol (MCP) Server Integration

**Files:**
- Create: `tools/milfoy-mcp/package.json`
- Create: `tools/milfoy-mcp/index.js`
- Create: `tools/milfoy-mcp/README.md`
- Modify: `.cursor/mcp.json` or project MCP config examples

**Interfaces:**
- Exposes MCP Tool: `get_ui_depth_diagnostics`
  - Arguments: `mode` (`anomalies`, `compact`), `format` (`md`, `json`)
  - Logic: First checks HTTP daemon (`http://127.0.0.1:8765`). If offline, falls back to reading `.milfoy/ui-context.md`.
- **Why this works:** When an agent inspects its toolset, `get_ui_depth_diagnostics` appears as a first-class native tool alongside file read/grep tools.

- [ ] **Step 1: Scaffold lightweight Node/TypeScript MCP server using `@modelcontextprotocol/sdk`**
- [ ] **Step 2: Implement `get_ui_depth_diagnostics` tool handler (HTTP call with disk fallback)**
- [ ] **Step 3: Provide auto-registration configuration for Cursor, Claude Desktop, and Antigravity**
- [ ] **Step 4: Verify test query through MCP inspector / CLI**

---

### Task 4: Ambient Test Runner Warning Injector

**Files:**
- Create: `Assets/UIDepthInspector/Editor/Diagnostics/UIDiagnosticTestAssert.cs`
- Modify: `Assets/UIDepthInspector/Tests/Editor/UIDepthInspector.Editor.Tests.asmdef`

**Interfaces:**
- Produces: Automated test assertion or warning log during test execution:
  - When tests run, if critical `DiagnosticFlags.OcclusionBlocker` or `DiagnosticFlags.GhostBlocker` exist, output formatted test warnings to the console/test log.
- **Why this works:** AI agents routinely run tests (`-runTests` or test runner tools). Seeing `[Milfoy Warning] BackgroundImage blocks Button` in the test output forces immediate natural engagement without prior instructions.

- [ ] **Step 1: Write `UIDiagnosticTestAssert` that evaluates all active root Canvases**
- [ ] **Step 2: Format failures as standard NUnit test assertions with exact remediation guidance**
- [ ] **Step 3: Verify test run surfaces UI blockers directly in test results XML**

---

### Task 5: Synchronization & Verification across Projects

**Files:**
- Sync: `Assets/UIDepthInspector` $\rightarrow$ `D:\Unity\Summer\Packages\com.canka.milfoy-ui`
- Verify: Test on `DemoScene.unity` and `Summer` project

- [ ] **Step 1: Sync all package files to target project**
- [ ] **Step 2: Verify `.milfoy/ui-context.md` is generated automatically upon saving a scene**
- [ ] **Step 3: Confirm visiting AI agents reach for the generated file or MCP tool naturally**
