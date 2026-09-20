# 🚀 Milfoy Growth & Audience Reach Playbook

This document is the actionable distribution, audience acquisition, and open-source contribution strategy for **milfoy-ui** (`com.canka.milfoy-ui`).

---

## 🎯 Target Audiences & Core Value Propositions

Milfoy has a rare **dual-hook** appeal that caters to two fast-growing developer segments:

### 1. The Unity Game Developer / UI Tech Artist (Visual Hook)
* **Pain Point**: "Why is my button not clicking? Why is this popup intercepting touch? Why does my multi-canvas sorting order look broken?"
* **Hook**: **Visual 3D Exploded Canvas Viewport** + Instant Diagnostic Badges (Ghost Blockers, Missing Sprites, Nested Raycasts).
* **Emotional Reaction**: *"I can see the invisible blocker floating right in front of my button in 3D."*

### 2. The AI-Assisted Developer / Tool Builder (Token & Speed Hook)
* **Pain Point**: AI coding assistants (Cursor, Claude Code, Antigravity, GitHub Copilot) burn 18,000+ tokens reading massive `.unity` scene YAMLs and still hallucinate screen-space touch raycasts.
* **Hook**: **Zero-overhead passive diagnostic cache** (`.milfoy/ui-context.md`) that delivers instant uGUI spatial intelligence in <300 tokens (-94.2% token cost, 4.7s fix time vs 79.5s).
* **Emotional Reaction**: *"My AI agent instantly fixed the UI bug without reading scene files or burning my API budget."*

---

## 📢 Multi-Channel Distribution Matrix

| Channel | Format | Primary Angle | Call to Action |
| :--- | :--- | :--- | :--- |
| **Reddit (`r/Unity3D`, `r/gamedev`)** | 10-second GIF / Video + Short Story | Visual 3D Exploded View + Ghost Blocker debugging | "Grab it on OpenUPM or GitHub (MIT)" |
| **Hacker News (Show HN)** | Text + Benchmark Data + Link | Technical breakdown of AI Token reduction (-94.2%) in Unity | "Looking for feedback on our passive cache architecture" |
| **X / Twitter & Bluesky** | Short video / thread with GIF | "The #1 reason your Unity button is unclickable" | Retweet + Star on GitHub |
| **Unity Discussions / Forums** | Dedicated Tool Thread | Comprehensive tool guide + OpenUPM package | Thread discussion & feature requests |
| **AI Developer Communities (Cursor / Claude / Latent Space Discord)** | Benchmark comparison post | How to make AI coding agents 15x faster at Unity UI bugs | Try `AGENTS.md` integration |
| **YouTube Shorts / TikTok** | 30-sec fast-paced UI debugging reel | Satisfying 3D canvas separation slider | Link in description |

---

## 🛠️ Step-by-Step Community Launch Sequence

### Phase 1: OpenUPM & GitHub Polish (Day 1)
1. Ensure GitHub repository has tags: `unity`, `unity3d`, `ugui`, `unity-editor`, `developer-tools`, `ai-agents`, `zero-gc`, `openupm`.
2. Submit package to **OpenUPM registry** via PR to `openupm/openupm`.
3. Pin high-resolution GIF banner at top of README.

### Phase 2: Visual Launch on Reddit & Socials (Day 2 - 3)
1. Post high-framerate video to `r/Unity3D`:
   - Title: *"We built a 3D exploded viewport for Unity uGUI to visually catch ghost raycast blockers and broken canvas depth [Open Source / MIT]"*
   - Top comment: Link to GitHub, explanation of zero-GC architecture and OpenUPM command.
2. Post short video thread on X / Twitter tagging `#unity3d #gamedev #madewithunity`.

### Phase 3: Technical AI & Benchmark Launch (Day 4 - 5)
1. Post **Show HN: Milfoy – Zero-overhead UI diagnostic cache that cuts AI agent tokens by 94% in Unity**:
   - Focus on the engineering: passive file cache on save (0% idle CPU), screen-space touch math in C#, and dual-agent benchmark results.
2. Share benchmark findings in Cursor & AI Agent developer communities.

---

## 🤝 Building a Thriving Contributor Funnel

To convert passive users into active open-source contributors:

1. **Curate "Good First Issue" Tracks**:
   - **Diagnostic Rules**: Add new UI sanity checks (e.g., detect oversized Mask components, non-integer pixel rects causing blurry text).
   - **Benchmark Presets**: Add realistic mobile game HUD presets to `UIBenchmarkPreset.cs`.
   - **Export Formats**: Add integrations for new AI agent frameworks (e.g., LangChain, custom MCP servers).
2. **Remove Scene Merge Friction**:
   - Encourage code contributions inside `Assets/UIDepthInspector/Editor/` which are pure C# and can be verified via headless automated tests (`UIBenchmarkTests.cs`, `AutoTestRunner.cs`).
3. **Reward Contributors**:
   - Prominently feature community contributors in `README.md` and release notes.
