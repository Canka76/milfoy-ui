# Milfoy UI Branding, README & Media Assets Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Establish GitHub repository branding for `milfoy-ui`, convert the mascot video to an optimized animated GIF banner, store the logo, and create a comprehensive, engaging README.

**Architecture:** Build media assets under `.github/assets/`, generate high-quality GIF with ffmpeg palettegen/paletteuse filters, and author standard GitHub + OpenUPM documentation.

**Tech Stack:** ffmpeg, markdown, git.

## Global Constraints
- Target Repo Name: `milfoy-ui`
- Media directory: `.github/assets/`
- Video source: `C:\Users\karak\Downloads\Milfoy_GIF.mp4`

---

### Task 1: Generate Branded Media Assets

**Files:**
- Create: `.github/assets/banner.gif`
- Create: `.github/assets/logo.png`

- [ ] **Step 1: Create assets directory**
```bash
mkdir -p .github/assets
```

- [ ] **Step 2: Convert MP4 video into optimized animated GIF with ffmpeg palette**
```bash
ffmpeg -y -i "C:/Users/karak/Downloads/Milfoy_GIF.mp4" -vf "fps=15,scale=960:-1:flags=lanczos,split[s0][s1];[s0]palettegen=max_colors=128[p];[s1][p]paletteuse=dither=bayer" .github/assets/banner.gif
```

- [ ] **Step 3: Extract / copy logo asset to .github/assets/logo.png**
Extract the high-res mascot avatar directly into `.github/assets/logo.png`.

- [ ] **Step 4: Verify media files exist and have non-zero size**
```bash
ls -la .github/assets/
```

- [ ] **Step 5: Commit media assets**
```bash
git add .github/assets/
git commit -m "chore(branding): add milfoy-ui mascot logo and animated banner gif"
```

---

### Task 2: Author Root README.md & Package Documentation

**Files:**
- Modify: `README.md`
- Modify: `Packages/com.openupm.ui-depth-inspector/README.md`
- Modify: `Assets/UIDepthInspector/README.md`

- [ ] **Step 1: Write root README.md**
Include mascot logo header, badges, animated demo banner, feature highlights (3D exploded view, real-time diagnostic warnings, custom color coding, neon brackets), installation instructions, keyboard shortcuts, and governance links.

- [ ] **Step 2: Sync package README files**
Ensure `Assets/UIDepthInspector/README.md` and `Packages/com.openupm.ui-depth-inspector/README.md` contain matching documentation and image references.

- [ ] **Step 3: Commit documentation**
```bash
git add README.md Assets/UIDepthInspector/README.md Packages/com.openupm.ui-depth-inspector/README.md
git commit -m "docs: author comprehensive milfoy-ui README with animated banner and mascot branding"
```
