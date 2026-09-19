# Design Spec: Comprehensive Documentation & Repository Alignment

**Date:** 2026-09-19  
**Status:** Approved (Pending Implementation Plan)  
**Target Files:**
- `README.md` (Root repository)
- `Assets/UIDepthInspector/README.md` (UPM Package documentation)
- `Assets/UIDepthInspector/CHANGELOG.md` (Package changelog)
- `AGENTS.md` (Agent guidance and benchmark references)

---

## 1. Executive Summary & Goals
Milfoy has evolved from a standalone 3D depth inspector into a dual-purpose developer and AI diagnostic engine with an integrated Benchmark Lab.

The goal of this update is to achieve complete, high-quality documentation alignment across the repository:
1. **Synchronize Root & Package READMEs**: Both `README.md` and `Assets/UIDepthInspector/README.md` present the synchronized Dual-Hero narrative (Visual 3D Inspector for human developers + Zero-Overhead AI Diagnostic Engine for AI coding agents).
2. **Highlight Empirical Proof**: Include the empirical benchmark metrics (**-94% tokens, 100% precision, 0 false positives**) with link to `docs/BENCHMARK_RESULTS.md`.
3. **Formalize Package CHANGELOG**: Populate `Assets/UIDepthInspector/CHANGELOG.md` following Keep a Changelog standards for OpenUPM release `v0.1.0`.
4. **Refine AI Agent Context**: Ensure `AGENTS.md` accurately documents passive caching, spatial occlusion queries, and benchmark trial execution.

---

## 2. Detailed File Specifications

### 2.1. Root `README.md` & `Assets/UIDepthInspector/README.md`
Both files will share identical content (with relative asset URLs adapted where appropriate):

- **Header Badges**:
  - Unity 6000.0+ / 2022.3+
  - OpenUPM badge
  - MIT License badge
  - PRs Welcome badge
  - **New:** `AI Token Savings: -94%` badge (`https://img.shields.io/badge/AI%20Tokens--94%25-brightgreen.svg?logo=openai`)
  - **New:** `Zero--GC: 0% Idle RAM-blue.svg`
- **Hero Overview**:
  - Positions Milfoy for both human developers and autonomous AI coding agents (Cursor, Claude, Copilot, Antigravity).
- **Core Feature Sections**:
  1. **Interactive 3D Viewport**: Exploded Z-layer separation, smooth camera navigation, solo mode, orientation badges.
  2. **Real-Time Diagnostic Badges**: Ghost blockers, zero-size rects, missing sprites, nested label raycasts, CanvasGroup traps.
  3. **Spatial Occlusion Analysis**: C# 2D screen-space pixel overlap computation across overlapping Canvases.
  4. **Zero-Overhead AI Passive Cache**: `.milfoy/ui-context.md` (<300 tokens) and `.json` auto-generated on scene save.
  5. **Benchmark Lab (`Tools > Milfoy > Benchmark Lab`)**: Procedural UI generator with 5 anomaly categories and dual-agent trial harness.
- **Empirical Scoreboard**:
  - Compact side-by-side table comparing Milfoy vs. Baseline agent (1,070 tokens vs. 18,040 tokens, 4.7s vs. 79.5s, 100% precision).
  - Link to `docs/BENCHMARK_RESULTS.md`.
- **Installation & Quick Start**:
  - OpenUPM CLI and Unity Package Manager Git URL.
  - Keyboard cheatsheet.
- **Headless Batchmode CLI**:
  - Commands for `DumpContext`, `GenerateBenchmark`, and `EvaluateBenchmark`.

### 2.2. Package Changelog (`Assets/UIDepthInspector/CHANGELOG.md`)
Populates `[0.1.0] - 2026-09-19` with categorized entries:
- **Added**:
  - 3D Exploded Viewport with configurable layer spacing and solo element inspection.
  - Comprehensive diagnostic analyzer (Ghost blockers, missing sprites, zero-size rects, nested labels).
  - Spatial occlusion analyzer detecting touch-stealing layers across overlapping Canvases with exact pixel overlaps.
  - Passive AI diagnostic cache (`.milfoy/ui-context.md`, `.milfoy/ui-context.json`) on scene save with 0% idle overhead.
  - Benchmark Lab with deterministic procedural UI generator, 5 anomaly categories, and headless batchmode runner.
  - Bi-directional 3-way synchronization between Viewport, Hierarchy, and Stack List.
  - Custom element color persistence and neon selection glow.
  - 30-test automated verification suite (`AutoTestRunner.cs`).

### 2.3. `AGENTS.md` Alignment
- Ensure all references to `.milfoy/ui-context.md` reflect current formatting and spatial occlusion badges.
- Cross-reference `docs/BENCHMARK_RESULTS.md` for AI benchmarks.

---

## 3. Verification & Review
1. Markdown lint and link validation across all changed documents.
2. Verify package README renders cleanly for OpenUPM publication.
