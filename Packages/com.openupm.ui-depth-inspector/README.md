<p align="center">
  <img src="https://raw.githubusercontent.com/Canka76/milfoy-ui/main/.github/assets/logo.png" alt="milfoy-ui logo" width="140"/>
</p>

<h1 align="center">milfoy-ui</h1>

<p align="center">
  <b>Interactive 3D Exploded Viewport & Depth Inspector for Unity uGUI</b>
</p>

<p align="center">
  <a href="https://unity.com"><img src="https://img.shields.io/badge/Unity-6000.0%2B-blue.svg?logo=unity" alt="Unity 6000.0+"/></a>
  <a href="https://package.openupm.com"><img src="https://img.shields.io/npm/v/com.openupm.ui-depth-inspector?label=openupm&registry_uri=https://package.openupm.com" alt="OpenUPM"/></a>
  <a href="LICENSE.md"><img src="https://img.shields.io/badge/License-MIT-green.svg" alt="MIT License"/></a>
  <a href="https://github.com/Canka76/milfoy-ui/blob/main/CONTRIBUTING.md"><img src="https://img.shields.io/badge/PRs-welcome-brightgreen.svg" alt="PRs Welcome"/></a>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/Canka76/milfoy-ui/main/.github/assets/banner.gif" alt="milfoy-ui banner" width="100%"/>
</p>

---

## 📖 Overview

**milfoy-ui** is a dockable Unity Editor tool that projects flat 2D uGUI Canvases into an interactive 3D exploded viewport. It provides deep visual debugging for draw order, raycast blockers, mask boundaries, depth sorting, and invisible UI traps—both in **Edit Mode** and **Play Mode**.

Stop guessing why a button is unclickable or which invisible overlay is intercepting clicks. With **milfoy-ui**, your entire Canvas hierarchy is exploded into intuitive 3D layers with exact draw indices and real-time diagnostic flags.

---

## ✨ Key Capabilities & Features

### 🔍 3D Exploded Viewport
- **Real-Time 3D Layer Separation**: Dynamically separates overlapping UI elements along the Z-axis with configurable layer spacing.
- **Intuitive Camera Navigation**: Smooth orbit, pan, and zoom gestures with customizable perspective and orthographic camera views.
- **Front / Back Orientation Badges**: Immediate visual indicators identifying Canvas front vs. back orientation in 3D space.

### ⚠️ Real-Time Diagnostic Badges & Warning Drawer
- **Ghost Blocker Detection**: Instantly spots invisible elements (`alpha == 0` or transparent graphic) that have `raycastTarget = true` blocking clicks.
- **Mask & RectMask2D Bounds**: Visual wireframe boundaries illustrating clipping regions and masked hierarchies.
- **CanvasGroup & Hierarchy Propagation**: Reflects active state, alpha inheritance, and raycast blocking cascades.

### 🎨 Custom Color Coding & Neon Selection Glow
- **Per-Element Custom Colors**: Assign distinct colors to specific UI elements via a color picker with `SessionState` persistence.
- **Neon Corner Brackets**: High-visibility glowing selection frame around the active UI element.
- **Background Dimming**: Non-selected layers gently dim to spotlight the focused element in complex hierarchies.

### 🔄 Bi-Directional 3-Way Synchronization
- Seamless selection sync between **3D Viewport ↔ Unity Hierarchy ↔ Draw-Order Stack List**.
- Selecting an item in any view automatically updates the selection across all views in real time.

---

## 📦 Installation

### Option 1: Via OpenUPM CLI (Recommended)

```bash
openupm add com.openupm.ui-depth-inspector
```

Or add via OpenUPM scoped registry in your `Packages/manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.openupm.ui-depth-inspector",
        "com.canka.milfoy-ui"
      ]
    }
  ],
  "dependencies": {
    "com.openupm.ui-depth-inspector": "0.1.0"
  }
}
```

### Option 2: Via Unity Package Manager (Git URL)

1. In Unity, open **Window → Package Manager**.
2. Click the **`+`** icon in the top-left corner.
3. Select **Add package from git URL...**
4. Enter:
   ```text
   https://github.com/canka/milfoy-ui.git
   ```

Or append directly to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.openupm.ui-depth-inspector": "https://github.com/canka/milfoy-ui.git#v0.1.0"
  }
}
```

---

## 🚀 Quick Start & Usage

1. Open the inspector via the Unity menu:
   ```text
   Window → UI → UI Depth Inspector
   ```
2. Select any GameObject containing a `Canvas` or click **Refresh** in the inspector toolbar.
3. Adjust the **Layer Spacing (Z)** slider in the toolbar to spread out the UI hierarchy in 3D.
4. Click any UI element in the 3D viewport, stack list, or Hierarchy window to inspect properties.

---

## 🎮 Controls & Keyboard Cheatsheet

| Input | Action |
|---|---|
| **Left Mouse Drag** (or **Alt + Left Drag**) | Orbit 3D camera around the Canvas center |
| **Alt + Left Drag** / **Middle Mouse Drag** | Pan viewport camera |
| **Scroll Wheel** | Zoom camera in / out |
| **Left Click** | Select UI element (synchronized across 3D Viewport, Hierarchy, and Stack List) |
| **`F`** | **Frame Selected**: Centers camera on the currently selected UI element |
| **`Escape`** | **Exit Solo Mode**: Returns to full Canvas view from isolated element mode |

---

## 🏗️ Architecture & Zero-GC Design

- **Non-Allocating Tree Collection**: `UIRenderTreeCollector` traverses uGUI render hierarchy with reusable pooled lists to eliminate per-frame GC allocations.
- **Dirty-Checking Cache**: `UIRenderTreeCache` avoids re-collecting trees when Canvas transform hierarchies are stable.
- **Native Unlit Diagnostic Shaders**: Fast custom shader (`UIQuadDiagnostic.shader`) renders diagnostic stripes and highlights in a single pass.
- **Isolated Preview Utility**: Uses `PreviewRenderUtility` with an isolated scene and dedicated camera, avoiding pollution of user scenes.

---

## 🤝 Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](https://github.com/Canka76/milfoy-ui/blob/main/CONTRIBUTING.md) for guidelines on branch naming, atomic commit formats, and the PR review process.

---

## 📄 License

This project is licensed under the terms of the [MIT License](LICENSE.md).
