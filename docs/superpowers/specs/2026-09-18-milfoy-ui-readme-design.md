# Design Spec: Milfoy UI Branding, README & Media Assets

## Goal
Establish repository identity for `milfoy-ui` with branded media assets (mascot logo, animated banner GIF converted from `C:\Users\karak\Downloads\Milfoy_GIF.mp4`), a complete developer-focused README, and updated package metadata for GitHub and OpenUPM.

---

## 1. Asset Pipeline & Locations

### Media Assets
- **Banner Video / GIF**:
  - Source: `C:\Users\karak\Downloads\Milfoy_GIF.mp4` (960x360 animation).
  - Target: `.github/assets/banner.gif` (converted via ffmpeg with optimal color palette for crisp pixel rendering and fast web loading).
- **Logo**:
  - Target: `.github/assets/logo.png` (mascot character logo).

---

## 2. README Structure (`README.md` & Package Readme)

### Layout
1. **Hero & Mascot**:
   - Centered mascot logo (`logo.png`, ~140px width)
   - Title: `# milfoy-ui`
   - Tagline: *"Interactive 3D Exploded Viewport & Depth Inspector for Unity uGUI"*
   - Shields/Badges:
     - `Unity 6000.0+`
     - `OpenUPM`
     - `License: MIT`
     - `PRs Welcome`
   - Full-width animated banner (`banner.gif`)
2. **Key Capabilities**:
   - **3D Exploded Viewport**: Layer separation along Z-axis, real-time orbit/pan/zoom, front/back orientation badges.
   - **Real-Time Diagnostic Badges**: Instant detection of invisible raycast blockers (alpha=0), depth sorting anomalies, and mask boundary violations.
   - **Custom Color Assignment & Neon Selection**: Per-element color coding with SessionState persistence and focused neon brackets with non-selected dimming.
   - **Bi-directional Sync**: 3D Viewport ↔ Unity Hierarchy ↔ Draw-Order Stack List.
3. **Installation**:
   - OpenUPM CLI & Scoped Registry instructions.
   - Unity Package Manager Git URL instructions.
4. **Controls Cheatsheet**:
   - Orbit, Pan, Zoom, Focus (`F`), Solo Isolation (`Escape` to exit).
5. **Architecture & Zero-GC Guarantees**:
   - Non-allocating render tree collection, batchmode test runner, and native diagnostic unlit shaders.
6. **Contributing & Governance**:
   - Links to `CONTRIBUTING.md` and `LICENSE.md`.

---

## 3. Package & Repository Alignment
- Align package metadata:
  - Display Name: `Milfoy UI - 3D Depth Inspector`
  - Identifier: `com.canka.milfoy-ui`
- Ensure root `README.md` and package documentation are synchronized.
