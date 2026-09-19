# Implementation Plan: Zero-Overhead Passive AI Diagnostic Cache

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Permanently remove network/HTTP server code from Milfoy and refine the event-driven scene-save exporter to ensure strict 0% idle CPU, 0 bytes RAM, and zero background threads.

**Architecture:** 
1. Delete `UIAIContextServer.cs` to eliminate all background socket and thread overhead.
2. Streamline `UIAIContextAutoExporter.cs` to hook strictly into `EditorSceneManager.sceneSaved` (no background polling or selection churn).
3. Update `AGENTS.md` and package documentation to reflect the passive on-disk diagnostic workflow.
4. Sync package to `D:\Unity\Summer\Packages\com.canka.milfoy-ui`.

**Tech Stack:** C# (Unity 6000.0+), UnityEditor.SceneManagement.

## Global Constraints
- Strict 0% idle CPU and RAM footprint.
- Zero background threads, zero open ports/sockets.
- Markdown anomaly export must stay $\le 300$ tokens.

---

### Task 1: Delete HTTP Server & Clean Up Networking Code

**Files:**
- Delete: `Assets/UIDepthInspector/Editor/Export/UIAIContextServer.cs`
- Delete: `D:/Unity/Summer/Packages/com.canka.milfoy-ui/Editor/Export/UIAIContextServer.cs`

- [ ] **Step 1: Delete `UIAIContextServer.cs` from working repo and target project**
- [ ] **Step 2: Confirm no dangling references exist in asmdef or window scripts**

---

### Task 2: Streamline `UIAIContextAutoExporter.cs` to Pure Event-Driven Scene Save

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Export/UIAIContextAutoExporter.cs`
- Test: `Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs`

- [ ] **Step 1: Remove any background selection listeners or timers, keeping strictly `EditorSceneManager.sceneSaved`**
- [ ] **Step 2: Ensure export directory `.milfoy/` is created cleanly and safely**
- [ ] **Step 3: Verify execution takes <3ms during scene save and 0ms at all other times**

---

### Task 3: Update `AGENTS.md` & Documentation

**Files:**
- Modify: `AGENTS.md`
- Copy: `AGENTS.md` $\rightarrow$ `D:\Unity\Summer\AGENTS.md`
- Modify: `Assets/UIDepthInspector/README.md`

- [ ] **Step 1: Remove all curl/port 8765 instructions from `AGENTS.md`**
- [ ] **Step 2: Add clear instructions pointing agents directly to `.milfoy/ui-context.md`**
- [ ] **Step 3: Sync all changes to `D:\Unity\Summer`**
