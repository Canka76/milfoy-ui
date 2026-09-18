# UI Layer & 3D Depth Inspector

A dockable Unity EditorWindow that renders flat uGUI Canvases as an interactive 3D exploded viewport for visual debugging of draw order, raycast blockers, mask boundaries, and occlusion — in Edit Mode and Play Mode.

## Features

- **3D Exploded Viewport** — Orbit, pan, zoom through your Canvas layers spread along the Z-axis.
- **Exact Draw Order** — Stack indices (#00–#N) matching Unity's actual render pipeline.
- **Ghost Blocker Detection** — Flags invisible elements (`alpha == 0`) that still block raycasts.
- **Mask Bounds** — Wireframe display of Mask and RectMask2D clip regions.
- **Quick Controls** — Per-element active toggle, raycast toggle, and solo isolation.
- **Selection Sync** — Click in 3D viewport ↔ Hierarchy ↔ Stack list, all synchronized.

## Requirements

- Unity 6 (6000.0+)
- uGUI (`com.unity.ugui` 2.0.0+)

## Installation

### Via OpenUPM (recommended)

```bash
openupm add com.openupm.ui-depth-inspector
```

### Via Git URL

Add to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.openupm.ui-depth-inspector": "https://github.com/openupm/ui-depth-inspector.git#v0.1.0"
  }
}
```

## Usage

Open via **Window → UI → UI Depth Inspector**.

## License

[MIT](LICENSE.md)
