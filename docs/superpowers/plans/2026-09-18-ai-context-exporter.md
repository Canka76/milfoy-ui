# AI Context Exporter & CLI Diagnostics Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Provide token-dense, actionable UI diagnostic summaries in Markdown (<500 tokens) and JSON format, accessible via an Editor Toolbar button and a headless CLI batchmode command for AI agents.

**Architecture:** A static formatting engine `UIAIContextExporter` in `UIDepthInspector.Editor.Export` converts `UIElementEntry` snapshots into compact Markdown or JSON. `UIToolbarPanel` exposes a one-click clipboard copy action with user feedback. `UIDepthInspectorCLI` exposes a static method `DumpContext` for headless batchmode queries.

**Tech Stack:** C# 9.0, Unity UI (uGUI), Unity Editor (`UnityEditor`, `UnityEditor.ShortcutManagement`), NUnit (`UnityEngine.TestTools`).

## Global Constraints
- Target Unity 6+ (6000.x) with URP.
- Assembly Definition: `UIDepthInspector.Editor` for editor code, `UIDepthInspector.Editor.Tests` for tests.
- Zero-allocation string builder / token-budget optimized (<500 tokens for Markdown hierarchy).
- Must execute cleanly in headless batchmode (`-executeMethod`).

---

### Task 1: Core Formatter (`UIAIContextExporter`) & Markdown / JSON Generation

**Files:**
- Create: `Assets/UIDepthInspector/Editor/Export/UIAIContextExporter.cs`
- Create: `Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs`

**Interfaces:**
- Produces:
  ```csharp
  namespace UIDepthInspector.Editor.Export
  {
      public static class UIAIContextExporter
      {
          public static string ExportToCompactMarkdown(IReadOnlyList<UIElementEntry> entries, Canvas rootCanvas);
          public static string ExportToJson(IReadOnlyList<UIElementEntry> entries, Canvas rootCanvas, bool prettyPrint = true);
      }
  }
  ```

- [ ] **Step 1: Write failing unit test for `UIAIContextExporter`**
Create `Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs` covering compact Markdown hierarchy generation, flag badge annotations, and JSON export.

- [ ] **Step 2: Implement `UIAIContextExporter`**
Create `Assets/UIDepthInspector/Editor/Export/UIAIContextExporter.cs` with:
- Indented ASCII tree hierarchy generation (`├── ` / `└── `)
- Draw order index mapping (`[#01]`)
- Shorthand diagnostic flags (`[GHOST_BLOCKER]`, `[RAYCAST]`, `[MASK]`, `[ZERO_SIZE]`)
- Remediation advice section at the end of the markdown summary
- JSON serialization mapping

- [ ] **Step 3: Run tests and verify they pass**
Verify compilation in Unity Editor log (`Logs/Editor.log`).

- [ ] **Step 4: Commit**
```bash
git add Assets/UIDepthInspector/Editor/Export/UIAIContextExporter.cs Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs
git commit -m "feat: add UIAIContextExporter for token-dense markdown and JSON export"
```

---

### Task 2: Editor GUI Toolbar Integration (`UIToolbarPanel`)

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Panels/UIToolbarPanel.cs`
- Modify: `Assets/UIDepthInspector/Editor/UIDepthInspectorWindow.cs`
- Modify: `Assets/UIDepthInspector/Editor/Resources/UIDepthInspector.uss`

**Interfaces:**
- Consumes: `UIAIContextExporter.ExportToCompactMarkdown`, `UIAIContextExporter.ExportToJson`

- [ ] **Step 1: Add Copy AI Prompt button in `UIToolbarPanel`**
Add a styled `Button` ("Copy AI Prompt") to the toolbar panel.
- Left-click: Copies compact markdown to `EditorGUIUtility.systemCopyBuffer` and shows feedback tooltip/status.
- Shift-click: Copies JSON dump to clipboard.

- [ ] **Step 2: Update USS styling**
Add styling in `UIDepthInspector.uss` for the AI copy button (distinctive cyan/indigo accent).

- [ ] **Step 3: Verify compilation in Editor**
Check `Logs/Editor.log` to confirm assembly reload.

- [ ] **Step 4: Commit**
```bash
git add Assets/UIDepthInspector/Editor/Panels/UIToolbarPanel.cs Assets/UIDepthInspector/Editor/Resources/UIDepthInspector.uss Assets/UIDepthInspector/Editor/UIDepthInspectorWindow.cs
git commit -m "feat: add Copy AI Prompt button to UI Depth Inspector toolbar"
```

---

### Task 3: Headless CLI Batchmode Command (`UIDepthInspectorCLI`)

**Files:**
- Create: `Assets/UIDepthInspector/Editor/Export/UIDepthInspectorCLI.cs`
- Modify: `Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs`

**Interfaces:**
- Produces:
  ```csharp
  namespace UIDepthInspector.Editor.Export
  {
      public static class UIDepthInspectorCLI
      {
          public static void DumpContext();
      }
  }
  ```

- [ ] **Step 1: Write test for CLI argument parsing**
Add unit test in `UIAIContextExporterTests.cs` validating argument parsing (`-dumpFormat`, `-dumpOutput`, `-canvasName`).

- [ ] **Step 2: Implement `UIDepthInspectorCLI.DumpContext`**
Create `Assets/UIDepthInspector/Editor/Export/UIDepthInspectorCLI.cs` with:
- `System.Environment.GetCommandLineArgs()` parsing
- Active Canvas discovery via `Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)`
- Invocation of `UIRenderTreeCollector.CollectTree` + `UIDiagnosticAnalyzer.Analyze`
- Writing output to file (`-dumpOutput`) or logging to standard output with delimiter tags `=== BEGIN UI AI CONTEXT ===` / `=== END UI AI CONTEXT ===`.

- [ ] **Step 3: Verify batch runner compilation and test execution**
Verify compilation in `Logs/Editor.log`.

- [ ] **Step 4: Commit**
```bash
git add Assets/UIDepthInspector/Editor/Export/UIDepthInspectorCLI.cs Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs
git commit -m "feat: add UIDepthInspectorCLI for headless batchmode AI diagnostic dumps"
```
