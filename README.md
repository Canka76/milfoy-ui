<p align="center">
  <img src="https://raw.githubusercontent.com/Canka76/milfoy-ui/main/.github/assets/logo.png" alt="milfoy-ui logo" width="140"/>
</p>

<h1 align="center">milfoy-ui</h1>

<p align="center">
  <b>Interactive 3D Exploded Viewport & AI Diagnostic Engine for Unity uGUI</b>
</p>

<p align="center">
  <a href="https://unity.com"><img src="https://img.shields.io/badge/Unity-6000.0%2B%20%7C%202022.3%2B-blue.svg?logo=unity" alt="Unity 6000.0+ | 2022.3+"/></a>
  <a href="https://package.openupm.com"><img src="https://img.shields.io/npm/v/com.canka.milfoy-ui?label=openupm&registry_uri=https://package.openupm.com" alt="OpenUPM"/></a>
  <a href="LICENSE.md"><img src="https://img.shields.io/badge/License-MIT-green.svg" alt="MIT License"/></a>
  <a href="https://github.com/Canka76/milfoy-ui/blob/main/docs/BENCHMARK_RESULTS.md"><img src="https://img.shields.io/badge/AI%20Tokens--94.2%25-brightgreen.svg?logo=openai" alt="AI Tokens -94%"/></a>
  <img src="https://img.shields.io/badge/Zero--GC-0%20B%2Fframe-blue.svg" alt="Zero-GC"/>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/Canka76/milfoy-ui/main/.github/assets/banner.gif" alt="milfoy-ui banner" width="100%"/>
</p>

---

## 📖 Overview

**milfoy-ui** is a zero-overhead Unity Editor tool and AI diagnostic engine that transforms flat, tangled 2D uGUI Canvases into an interactive 3D exploded viewport—while simultaneously equipping AI coding agents (Cursor, Claude, Copilot, Antigravity) with token-optimized UI diagnostic intelligence.

### 🎮 For Game Developers: Visual 3D Exploded Inspection
Stop guessing why a button is unclickable, which invisible overlay is intercepting touches, or how nested Canvases are sorted. Milfoy separates your UI elements along the Z-axis in an intuitive 3D camera view with real-time diagnostic flags, spatial occlusion heatmaps, and bi-directional 3-way selection synchronization.

### 🤖 For AI Coding Agents: Zero-Overhead Diagnostic Cache
Analyzing complex `.unity` scene YAML files burns tens of thousands of tokens and often causes AI agents to hallucinate screen-space touch coordinates. Whenever you save a scene in Unity, Milfoy passively pre-computes hierarchy bounds, raycast blockers, and screen-space occlusions into `.milfoy/ui-context.md` (<300 tokens) with **0% idle CPU/RAM overhead**, cutting AI token consumption by **94.2%**.

---

## ⚡ Empirical Proof: Dual-Agent Benchmark Results

In rigorous, head-to-head empirical trials comparing Milfoy against a baseline AI agent navigating raw scene files across 4 complexity tiers (from clean hierarchies to 80+ element chaotic multi-canvas scenes):

| Metric | Baseline Agent (Raw Scene / Grep) | Milfoy-Equipped Agent | Efficiency Gain |
| :--- | :--- | :--- | :--- |
| **Total Tokens** | 18,040 tokens | **1,070 tokens** | **-94.1% Cost Reduction** |
| **Tool / Turn Count** | 14 turns | **2 turns** | **-85.7% Faster Iteration** |
| **Resolution Time** | 79.5 seconds | **4.7 seconds** | **-94.1% Latency Reduction** |
| **Diagnostic Precision** | 66.7% (Hallucinations) | **100.0% (Zero False Positives)** | **+33.3% Accuracy** |
| **Diagnostic Recall** | 66.7% (Missed Bugs) | **100.0% (Zero False Negatives)** | **+33.3% Defect Coverage** |
| **F1 Score** | 0.667 | **1.000 (Flawless)** | **+0.333** |

👉 **Full benchmark methodology, anomaly breakdown, and reproduction guide:** [docs/BENCHMARK_RESULTS.md](docs/BENCHMARK_RESULTS.md)

---

## ✨ Key Capabilities & Features

### 🔍 1. Interactive 3D Exploded Viewport
- **Z-Axis Layer Separation**: Smoothly spread UI layers along the depth axis using the **Layer Spacing (Z)** slider to inspect exact draw ordering.
- **Intuitive Camera Navigation**: Orbit (`Left Drag` or `Alt + Drag`), Pan (`Middle Drag`), and Zoom (`Scroll Wheel`) in perspective or orthographic modes.
- **Solo Mode (`Double Click` / `Space`)**: Isolate a single element in 3D while temporarily dimming the rest of the hierarchy.
- **Front / Back Orientation Badges**: Immediate visual indicators identifying Canvas front vs. back orientation in 3D space.

### ⚠️ 2. Real-Time Diagnostic Badges
- **Ghost Blocker Detection**: Instantly flags invisible elements (`alpha == 0` or transparent graphic) that have `raycastTarget = true` blocking clicks.
- **Spatial Occlusion Analysis**: Computes 2D screen-space touch intersections between overlapping Canvases and interactive buttons directly in C#, tagging occluders with `DiagnosticFlags.OcclusionBlocker` and providing exact pixel overlaps.
- **Missing Sprites & Zero-Size Rects**: Spots broken sprite references and `0x0` dimensions acting as active raycast targets.
- **Nested Label Raycasts**: Detects redundant `raycastTarget = true` on child `Text` or `TextMeshProUGUI` components inside buttons.
- **CanvasGroup Transparency Traps**: Identifies `alpha = 0` or non-interactable `CanvasGroup` components that still block raycasts.

### 🎨 3. Custom Color Coding & Neon Selection Glow
- **Session-Persistent Color Coding**: Assign distinct colors to specific UI elements via inline color pickers with `SessionState` persistence.
- **Neon Corner Brackets**: High-visibility glowing selection frame around the active UI element.
- **Background Layer Dimming**: Non-selected layers gently dim to spotlight the focused element in deep hierarchies.

### 🔄 4. Bi-Directional 3-Way Synchronization
- Seamless selection sync between **3D Viewport ↔ Unity Hierarchy ↔ Draw-Order Stack List**.
- Selecting an item in any view automatically updates the selection across all views in real time.

### 🧪 5. UI Benchmark Lab (`Tools > Milfoy > Benchmark Lab`)
- **Procedural UI Generator**: Programmatically generates realistic uGUI hierarchies with deterministic seeds.
- **5 Anomaly Injections**: Procedurally injects Ghost Blockers, Spatial Overlaps, Missing Sprites, Nested Raycasts, and CanvasGroup Traps.
- **Automated Dual-Agent Trial Runner**: Includes `tools/run_benchmark_trial.ps1` for continuous CI/CD evaluation of AI coding agent performance.

---

## 🚀 Quick Start & Installation

### Option A: Install via OpenUPM (Recommended)
```bash
openupm add com.canka.milfoy-ui
```

### Option B: Install via Unity Package Manager (Git URL)
1. Open the Unity Editor.
2. Navigate to **Window > Package Manager**.
3. Click the **`+`** icon in the top-left and select **Add package from git URL...**.
4. Enter:
   ```text
   https://github.com/Canka76/milfoy-ui.git?path=/Assets/UIDepthInspector
   ```

### Usage
1. Open the inspector via the Unity menu:
   - **`Window > UI > UI 3D Depth Inspector`** (or **`Tools > Milfoy > UI 3D Depth Inspector`**).
2. Select any GameObject in your Canvas hierarchy.
3. Adjust the **Layer Spacing (Z)** slider in the toolbar to spread out the UI hierarchy in 3D.
4. Click any UI element in the 3D viewport, stack list, or Hierarchy window to inspect properties and diagnostic badges.

---

## 🎮 Controls & Keyboard Cheatsheet

| Input | Action |
|---|---|
| **Left Mouse Drag** (or **Alt + Left Drag**) | Orbit 3D camera around the Canvas center |
| **Alt + Left Drag** / **Middle Mouse Drag** | Pan viewport camera |
| **Scroll Wheel** | Zoom camera in / out |
| **Left Click** | Select UI element (synchronized across Viewport, Hierarchy, and Stack List) |
| **`F`** | **Frame Selected**: Centers camera on the currently selected UI element |
| **`Escape`** | **Exit Solo Mode**: Returns to full Canvas view from isolated element mode |

---

## 🏗️ Architecture & Zero-GC Design

- **Non-Allocating Tree Collection**: `UIRenderTreeCollector` traverses the uGUI render hierarchy with reusable pooled lists to eliminate per-frame GC allocations.
- **Dirty-Checking Cache**: `UIRenderTreeCache` avoids re-collecting trees when Canvas transform hierarchies are stable.
- **Passive Event-Driven Exporter**: `UIAIContextAutoExporter` listens to `EditorSceneManager.sceneSaved` to maintain `.milfoy/ui-context.*` without background threads, HTTP servers, or polling loops.
- **Native Unlit Diagnostic Shaders**: Fast custom shader (`UIQuadDiagnostic.shader`) renders diagnostic stripes and highlights in a single pass.
- **Zero Player Overhead**: The entire package compiles strictly in Editor assemblies (`Editor/`). Zero bytes and zero scripts are included in standalone player builds.

---

## 🤖 Headless Batchmode CLI (CI / CD)

Milfoy provides parameterless batchmode entry points for automated workflows:

```bash
# Dump token-optimized AI context
Unity.exe -batchmode -quit -projectPath . \
  -executeMethod UIDepthInspector.Editor.Export.UIDepthInspectorCLI.DumpContext \
  -dumpFormat md \
  -dumpMode anomalies \
  -dumpOutput "ui-context.md"

# Generate procedural benchmark scene
Unity.exe -batchmode -quit -projectPath . \
  -executeMethod UIDepthInspector.Editor.Export.UIDepthInspectorCLI.GenerateBenchmark \
  -benchmarkPreset ChaoticStress \
  -benchmarkSeed 400 \
  -benchmarkOutDir "BenchmarkTrials/Run_Chaos_400"
```

---

## 📄 License

This project is licensed under the **MIT License**. See [LICENSE.md](LICENSE.md) for details.
