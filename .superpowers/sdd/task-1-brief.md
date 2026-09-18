# Task 1 Brief: Repository Scaffold & Governance Files

## Files to Create
- `package.json`
- `LICENSE.md`
- `README.md`
- `CHANGELOG.md`
- `CONTRIBUTING.md`
- `.editorconfig`
- `.gitignore`
- `.gitattributes`
- `.github/workflows/commitlint.yml`
- `.github/workflows/release-please.yml`
- `.github/PULL_REQUEST_TEMPLATE.md`
- `release-please-config.json`
- `.release-please-manifest.json`
- `Editor/UIDepthInspector.Editor.asmdef`

## Exact Contents

### `.gitignore`
```gitignore
# Unity project artifacts (test project alongside the package)
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/

# IDE
.idea/
.vs/
*.csproj
*.sln
*.user
*.suo
*.userprefs

# OS
.DS_Store
.DS_Store?
Thumbs.db
ehthumbs.db

# Node (if commitlint run locally by a contributor)
node_modules/
commitlint.config.js
package-lock.json
```

### `.gitattributes`
```gitattributes
# Force LF on all text files regardless of contributor OS
* text=auto eol=lf

*.cs      text eol=lf
*.json    text eol=lf
*.yaml    text eol=lf
*.yml     text eol=lf
*.md      text eol=lf
*.shader  text eol=lf
*.uxml    text eol=lf
*.uss     text eol=lf
*.asmdef  text eol=lf

# Unity binary formats — no line-ending conversion
*.png     binary
*.jpg     binary
*.psd     binary
*.asset   -text
*.meta    -text
*.prefab  -text
*.unity   -text
```

### `.editorconfig`
```ini
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true

[*.cs]
indent_style = space
indent_size = 4

[*.{json,yaml,yml,uxml,uss}]
indent_style = space
indent_size = 2

[*.md]
trim_trailing_whitespace = false
```

### `package.json`
```json
{
  "name": "com.openupm.ui-depth-inspector",
  "version": "0.1.0",
  "displayName": "UI Layer & 3D Depth Inspector",
  "description": "Interactive 3D exploded viewport for debugging uGUI Canvas draw order, raycast blockers, and mask boundaries directly inside the Unity Editor.",
  "unity": "6000.0",
  "unityRelease": "0f1",
  "documentationUrl": "https://github.com/openupm/ui-depth-inspector#readme",
  "changelogUrl": "https://github.com/openupm/ui-depth-inspector/blob/main/CHANGELOG.md",
  "licensesUrl": "https://github.com/openupm/ui-depth-inspector/blob/main/LICENSE.md",
  "keywords": [
    "ui",
    "ugui",
    "canvas",
    "debug",
    "inspector",
    "depth",
    "editor",
    "raycast",
    "3d"
  ],
  "author": {
    "name": "OpenUPM Community",
    "url": "https://github.com/openupm"
  },
  "dependencies": {
    "com.unity.ugui": "2.0.0"
  }
}
```

### `LICENSE.md`
```markdown
MIT License

Copyright (c) 2026 UI Depth Inspector Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

### `README.md`
```markdown
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
```

### `CHANGELOG.md`
```markdown
# Changelog

All notable changes to this project will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<!-- AUTOMATICALLY MANAGED BY release-please. DO NOT EDIT MANUALLY. -->
```

### `CONTRIBUTING.md`
```markdown
# Contributing to UI Depth Inspector

Thank you for considering a contribution!

## Quick Start

1. Fork this repository.
2. Create a branch from `main`: `feat/your-feature-name`.
3. Make your changes with atomic commits.
4. Open a Pull Request against `main`.

## Commit Rules

This project uses [Conventional Commits](https://www.conventionalcommits.org/). Every commit message must follow this format:

```
<type>(<optional scope>): <description>
```

### Allowed Types

| Type | Use for |
|------|---------|
| `feat` | New feature visible to users |
| `fix` | Bug fix |
| `docs` | README, CONTRIBUTING, XML doc comments |
| `refactor` | Internal restructure, no behavior change |
| `perf` | Performance improvement |
| `test` | Adding or fixing tests |
| `chore` | Tooling, CI, build scripts, dependency bumps |
| `style` | Formatting only |

### Rules

- **Imperative mood**, ≤72 characters, no trailing period.
  - ✓ `feat: add World Space canvas support`
  - ✗ `Added world space canvas support.`
- **Atomic commits:** one logical change per commit. A PR fixing a bug AND adding a feature needs at least two commits.
- **Scope** (optional): subsystem in parentheses — `fix(viewport): orbit camera jitter`.

### Breaking Changes

Append `!` after type and add a `BREAKING CHANGE:` footer:

```
feat!: rename UIElementEntry.worldRect to boundsRect

BREAKING CHANGE: worldRect has been renamed to boundsRect for clarity.
```

## Branch Naming

Format: `<type>/<short-description>` (kebab-case).

- `feat/world-space-canvas`
- `fix/ghost-blocker-alpha-calc`
- `docs/contributing-guide`

## PR Process

- Open an issue first for non-trivial features.
- Link the issue in your PR: `Closes #N`.
- CI must pass (commitlint checks commit messages).

## Unity Version

Test against **Unity 6 (6000.0.x)** minimum. Note your exact Unity version in the PR description.

## What NOT To Do

- **Do not** manually edit `CHANGELOG.md` — release-please manages it.
- **Do not** manually bump `version` in `package.json` — release-please handles it.

## Local Development

Install the package in a test project via local path override in `Packages/manifest.json`:

```json
"com.openupm.ui-depth-inspector": "file:../../path/to/ui-depth-inspector"
```
```

### `.github/PULL_REQUEST_TEMPLATE.md`
```markdown
## What does this PR do?
<!-- One sentence. -->

## Linked issue
Closes #

## Checklist
- [ ] Commits follow Conventional Commits format (`feat:`, `fix:`, `docs:`, etc.)
- [ ] Tested in Unity 6 (6000.0.x) — Edit Mode and/or Play Mode as applicable
- [ ] No new compiler warnings introduced
- [ ] `CHANGELOG.md` NOT manually edited — release-please manages it automatically
```

### `.github/workflows/commitlint.yml`
```yaml
name: commitlint

on:
  pull_request:
    types: [opened, synchronize, reopened]

jobs:
  commitlint:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - uses: actions/setup-node@v4
        with:
          node-version: lts/*

      - name: Install commitlint
        run: npm install --save-dev @commitlint/cli @commitlint/config-conventional

      - name: Create commitlint config
        run: echo "module.exports = { extends: ['@commitlint/config-conventional'] };" > commitlint.config.js

      - name: Lint commits
        run: npx commitlint --from ${{ github.event.pull_request.base.sha }} --to ${{ github.event.pull_request.head.sha }} --verbose

      - name: Lint PR title
        env:
          PR_TITLE: ${{ github.event.pull_request.title }}
        run: echo "$PR_TITLE" | npx commitlint --verbose
```

### `.github/workflows/release-please.yml`
```yaml
name: release-please

on:
  push:
    branches:
      - main

permissions:
  contents: write
  pull-requests: write

jobs:
  release-please:
    runs-on: ubuntu-latest
    steps:
      - uses: google-github-actions/release-please-action@v4
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
```

### `release-please-config.json`
```json
{
  "packages": {
    ".": {
      "release-type": "simple",
      "extra-files": [
        {
          "type": "json",
          "path": "package.json",
          "jsonpath": "$.version"
        }
      ]
    }
  }
}
```

### `.release-please-manifest.json`
```json
{
  ".": "0.1.0"
}
```

### `Editor/UIDepthInspector.Editor.asmdef`
```json
{
  "name": "UIDepthInspector.Editor",
  "rootNamespace": "UIDepthInspector.Editor",
  "references": [
    "UnityEngine.UI"
  ],
  "includePlatforms": [
    "Editor"
  ],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

## Commit Message
```bash
git add -A
git commit -m "chore: scaffold UPM package with governance and CI files"
```
