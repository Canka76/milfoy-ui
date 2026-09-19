# Milfoy AI Communication & Diagnostic Engine Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a token-efficient spatial occlusion diagnostic engine and machine-patchable AI action exporter in Milfoy UI Depth Inspector.

**Architecture:** Extend `UIDiagnosticAnalyzer` to compute 2D screen-space occlusion triplets over interactive elements across Canvases, add new `DiagnosticFlags`, update `UIAIContextExporter` with `anomalies`, `compact`, and `json` action modes, and expose mode toggles in `UIDepthInspectorCLI`.

**Tech Stack:** C#, Unity UGUI, Editor scripting, NUnit Test Framework.

## Global Constraints
- Target Unity 6+ UGUI runtime.
- Zero avoidable GC allocations in diagnostic loops.
- Do not break backward compatibility of existing CLI invocation commands.

---

### Task 1: Diagnostic Flags & Spatial Occlusion Data Structures

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Diagnostics/UIDiagnosticBadges.cs`
- Modify: `Assets/UIDepthInspector/Editor/Core/UIElementEntry.cs`
- Test: `Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs`

**Interfaces:**
- Produces: `DiagnosticFlags.OcclusionBlocker`, `DiagnosticFlags.NestedLabelRaycast`, `OcclusionPair` struct in `UIDiagnosticAnalyzer`.

- [ ] **Step 1: Add new enum flags in `UIDiagnosticBadges.cs`**
- [ ] **Step 2: Add `OcclusionPairs` list field to `UIElementEntry.cs`**
- [ ] **Step 3: Verify compilation in Editor test suite**

---

### Task 2: Spatial Occlusion Analysis Logic in `UIDiagnosticAnalyzer`

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Diagnostics/UIDiagnosticAnalyzer.cs`
- Test: `Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs`

**Interfaces:**
- Consumes: `List<UIElementEntry> entries`
- Produces: Populated `DiagnosticFlags.OcclusionBlocker` and `entry.OcclusionPairs` on occluding and occluded elements.

- [ ] **Step 1: Write unit test in `UIAIContextExporterTests.cs` for detecting overlapping elements across different Canvas sorting orders**
- [ ] **Step 2: Implement screen-space overlap check in `UIDiagnosticAnalyzer.cs`**
- [ ] **Step 3: Run tests to verify spatial occlusion detection**

---

### Task 3: Token-Optimized Markdown & JSON Action Exporter

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Export/UIAIContextExporter.cs`
- Test: `Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs`

**Interfaces:**
- Produces: `ExportToCompactMarkdown(entries, canvas, mode)` and `ExportToJson(entries, canvas, mode, prettyPrint)`.

- [ ] **Step 1: Write unit tests for `anomalies` mode in Markdown and JSON**
- [ ] **Step 2: Implement `anomalies` and `compact` filters in `UIAIContextExporter.cs`**
- [ ] **Step 3: Add `fixAction` JSON schema payload generator**
- [ ] **Step 4: Verify test suite execution**

---

### Task 4: Headless CLI & Window Integration

**Files:**
- Modify: `Assets/UIDepthInspector/Editor/Export/UIDepthInspectorCLI.cs`
- Modify: `Assets/UIDepthInspector/Editor/Panels/UIToolbarPanel.cs`
- Test: `Assets/UIDepthInspector/Tests/Editor/UIAIContextExporterTests.cs`

- [ ] **Step 1: Add `-dumpMode` parameter parsing in `UIDepthInspectorCLI.cs`**
- [ ] **Step 2: Update AI Copy button in `UIToolbarPanel.cs` to default to compact anomaly format**
- [ ] **Step 3: Run end-to-end batchmode test on `DemoScene.unity`**
