# OpenUPM Registration & Publishing Guide

## 1. Package Information
- **Package Name**: `com.canka.milfoy-ui`
- **Repository URL**: `https://github.com/Canka76/milfoy-ui`
- **Package Path**: `Assets/UIDepthInspector` (or root if using upm subtree branch)

---

## 2. OpenUPM Registration YAML

To register `milfoy-ui` on OpenUPM (so `openupm add com.canka.milfoy-ui` works):

1. Fork the official OpenUPM repository: [https://github.com/openupm/openupm](https://github.com/openupm/openupm)
2. Create a new file in your fork at `data/packages/com.canka.milfoy-ui.yml`:

```yaml
name: com.canka.milfoy-ui
displayName: Milfoy UI - 3D Depth Inspector
description: Interactive 3D exploded viewport for debugging uGUI Canvas draw order, raycast blockers, and mask boundaries directly inside the Unity Editor.
repoUrl: https://github.com/Canka76/milfoy-ui
parentRepoUrl: null
licenseSpdxId: MIT
licenseName: MIT License
topics:
  - ui
  - ugui
  - canvas
  - debug
  - editor
  - depth
hunter: Canka76
gitTagPrefix: v
gitTagIgnore: ''
minVersion: ''
```

3. Open a Pull Request from your fork to `openupm/openupm`.
4. Once merged (OpenUPM's automated bot typically validates and merges within hours), the build pipeline will crawl the repository releases/tags and publish `com.canka.milfoy-ui` to the OpenUPM registry!

---

## 3. Creating a Release Tag

OpenUPM builds packages from Git tags (`v0.1.0`, etc.):

```bash
git tag -a v0.1.0 -m "release: v0.1.0"
git push origin v0.1.0
```
