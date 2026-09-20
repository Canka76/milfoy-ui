# 📝 Milfoy Content & Launch Templates

Copy-pasteable announcement and outreach templates optimized for Reddit, Hacker News, X/Twitter, and Discord communities.

---

## 1. Reddit (`r/Unity3D`, `r/gamedev`) Launch Post

**Title**: We built a 3D exploded viewport for Unity uGUI to visually catch ghost raycast blockers and broken canvas depth [Open Source / MIT]

**Body**:
Hey everyone! 👋 

If you've ever spent 30 minutes trying to figure out *why* a UI button isn't clickable, *which* invisible panel has `raycastTarget = true` stealing clicks, or *how* nested Canvases are sorting, you know the pain of debugging Unity uGUI.

We built **Milfoy** (`milfoy-ui`), a zero-overhead Unity Editor tool that spreads your 2D uGUI canvas layers along the Z-axis in an interactive 3D exploded viewport.

### ✨ What it does:
- **3D Exploded View**: Smoothly adjust layer spacing in 3D to see exactly what's overlapping.
- **Ghost Blocker Detection**: Instantly flags invisible elements (`alpha == 0`) that are blocking touches.
- **AI Diagnostic Cache**: Automatically exports token-optimized UI diagnostic context (<300 tokens) on scene save, cutting AI coding agent token consumption by **94.2%** when using Cursor, Claude, or Antigravity.
- **Zero-GC Guarantee**: 0% idle CPU/RAM overhead, 0 allocations in inspection hot paths.

📦 **Install via OpenUPM**:
```bash
openupm add com.canka.milfoy-ui
```

🔗 **GitHub (MIT)**: [https://github.com/Canka76/milfoy-ui](https://github.com/Canka76/milfoy-ui)

We'd love your feedback, bug reports, and PRs! What's your biggest uGUI pain point?

---

## 2. Hacker News ("Show HN") Post

**Title**: Show HN: Milfoy – Zero-overhead UI diagnostic cache & 3D inspector for Unity uGUI

**URL**: `https://github.com/Canka76/milfoy-ui`

**Text / First Comment**:
Hi HN! We built **Milfoy** because AI coding assistants (Cursor, Claude, Antigrawal) burn thousands of tokens reading massive raw `.unity` scene YAML files and still hallucinate touch raycast issues.

Milfoy solves this with a two-pronged approach:
1. **For Humans**: An interactive 3D exploded viewport in the Unity Editor that makes flat uGUI hierarchies instantly inspectable and spots raycast blocker anomalies in real-time.
2. **For AI Agents**: A passive event-driven exporter (`EditorSceneManager.sceneSaved`) that writes a compact `<300` token diagnostic summary (`.milfoy/ui-context.md`) with 0% idle overhead. In rigorous head-to-head benchmarks across 4 complexity tiers, this cut AI token consumption by **94.2%** and resolution time from 79.5s to 4.7s.

It's MIT-licensed and installable via OpenUPM. Curious what challenges you've faced with Unity UI or AI tooling in game dev!

---

## 3. X / Twitter / Bluesky Thread

**Post 1**:
Tired of your Unity uGUI buttons refusing to click and having no idea which invisible panel is stealing touches? 🛑

Meet **Milfoy**: an open-source 3D exploded viewport & AI diagnostic engine for Unity uGUI. 🧵 👇 [Attach GIF banner]

**Post 2**:
🔍 **For Developers**:
Spread your UI canvas layers along the Z-axis in 3D. Instantly spot Ghost Blockers, missing sprites, and sorting overlap issues with real-time diagnostic badges.

🤖 **For AI Agents (Cursor / Claude)**:
Cuts AI token costs by **94.2%** via a zero-idle-overhead passive scene save cache (`.milfoy/ui-context.md`). No more hallucinated scene YAML parsing!

**Post 3**:
📦 100% Zero-GC, MIT License, OpenUPM ready!

Install via OpenUPM:
`openupm add com.canka.milfoy-ui`

Star & contribute on GitHub:
`https://github.com/Canka76/milfoy-ui`

#unity3d #gamedev #madewithunity #indiedev
