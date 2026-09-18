# Contribution Governance Design Spec

**Date:** 2026-09-18
**Status:** Draft
**Project:** `com.openupm.ui-depth-inspector`
**Audience:** Solo maintainer + occasional external PRs

---

## 1. Purpose

Establish lightweight, enforceable contribution rules for the open-source UPM package before the first public PR arrives. Goals:

- Keep git history clean and machine-readable (Conventional Commits → automated changelog).
- Prevent the most common open-source friction: bad commit messages, manual CHANGELOG edits, broken merges.
- Stay proportionate: no process heavier than the project size warrants.

---

## 2. Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Commit convention | Conventional Commits | Industry standard; enables automated changelog and semver bumping via release-please. |
| Enforcement | Light CI — commitlint on GitHub Actions | Catches bad commits at PR time without Unity licensing overhead. |
| Changelog | Automated via `release-please` | Zero manual changelog work; version bump + GitHub Release tag created automatically on merge to `main`. |
| Distribution | OpenUPM registry + Git URL | OpenUPM watches GitHub release tags (already created by release-please) — zero extra CI step. Discoverability vs. Git-URL-only is the difference between 10 users and 10,000. |
| Package namespace | `com.<yourgithuborg>.ui-depth-inspector` | `com.openupm` is OpenUPM's own domain; using it as a package namespace causes confusion and potential conflicts. Resolve to a clean reverse-domain before first publish. |
| Unity compilation gate | Deferred | Over-engineered for solo maintainer; add when a self-hosted runner or second regular contributor exists. |
| Community governance (CoC, issue templates, CODEOWNERS) | Deferred | Add incrementally when community grows. |

---

## 3. Repository File Layout

```
com.<yourgithuborg>.ui-depth-inspector/   (repo root)
├── .editorconfig
├── .gitattributes
├── .gitignore                     # Unity + OS + IDE artifacts (see Section 6)
├── .github/
│   ├── workflows/
│   │   ├── commitlint.yml         # PR commit validation
│   │   └── release-please.yml    # Automated CHANGELOG + version bump + GitHub Release
│   └── PULL_REQUEST_TEMPLATE.md
├── CHANGELOG.md                   # Seeded; managed by release-please
├── CONTRIBUTING.md
├── LICENSE.md                     # MIT
├── README.md
├── package.json                   # UPM manifest — full field spec in Section 6
├── release-please-config.json     # release-please: release-type + extra-files
├── .release-please-manifest.json  # release-please: current tracked version
└── Editor/
    └── ...
```

No `commitlint.config.js` or `package.json` at root — commitlint installed ad-hoc inside the GitHub Actions workflow to keep the repo free of Node.js artifacts.

---

## 4. Conventional Commits Rules

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
| `style` | Formatting only (whitespace, .editorconfig fixes) |

### Commit Message Rules

- **Subject line:** imperative mood, ≤72 characters, no trailing period.
  - ✓ `feat: add World Space canvas support`
  - ✗ `Added world space canvas support.`
- **Atomic commits:** one logical change per commit. A PR fixing a bug and adding a feature needs at least two commits.
- **Scope** (optional): subsystem in parentheses.
  - `fix(viewport): orbit camera jitter at low framerate`
  - `feat(diagnostics): add RectMask2D wireframe bounds`
- **Breaking changes:** append `!` after type, add `BREAKING CHANGE:` footer.
  - `feat!: rename UIElementEntry.worldRect to boundsRect`
  - This triggers a major version bump in release-please.

### Branch Naming Convention

Format: `<type>/<short-description>` (kebab-case).

| Branch | Example |
|--------|---------|
| `feat/<desc>` | `feat/world-space-canvas` |
| `fix/<desc>` | `fix/ghost-blocker-alpha-calc` |
| `docs/<desc>` | `docs/contributing-guide` |
| `chore/<desc>` | `chore/release-please-setup` |

Not CI-enforced; documented as convention only.

---

## 5. GitHub Actions Workflows

### `commitlint.yml` — PR Commit Validation

**Trigger:** `pull_request` events: opened, synchronize, reopened.

**Steps:**
1. Checkout with full history (`fetch-depth: 0`).
2. Install Node.js (LTS).
3. Install `@commitlint/cli` and `@commitlint/config-conventional` inline (no repo-level `package.json`).
4. Run commitlint over the PR's commit range:
   ```
   commitlint --from ${{ github.event.pull_request.base.sha }}
              --to   ${{ github.event.pull_request.head.sha }}
              --verbose
   ```
5. Also validate the PR title itself — squash merges land as a single commit using the PR title as the subject line.

**Outcome:** PR check fails if any commit message (or the PR title) violates the ruleset. Merge is blocked until fixed.

**Commitlint config** (inline in workflow, no config file in repo):
```yaml
echo "module.exports = { extends: ['@commitlint/config-conventional'] };" > commitlint.config.js
```

### `release-please.yml` — Automated Releases

**Trigger:** `push` to `main`.

**Action:** `google-github-actions/release-please-action@v4`

**Behavior:**
- Reads merged Conventional Commits since the last release tag.
- Maintains an open "Release PR" that accumulates `CHANGELOG.md` entries.
- When the Release PR is merged: creates a git tag (`v1.0.0`, `v1.1.0`, etc.) and a GitHub Release.
- Bumps `version` in `package.json` (semver: `feat` → minor, `fix` → patch, `feat!` → major).

**Version target:** `package.json` → `version` field. release-please is configured via an inline `release-please-config.json` committed to the repo root:
```json
{
  "release-type": "simple",
  "extra-files": ["package.json"]
}
```
And a `.release-please-manifest.json` tracking the current version:
```json
{ ".": "0.1.0" }
```

**UPM integration:** Contributors install specific releases by pinning the git tag in their project's `manifest.json`:
```json
"com.openupm.ui-depth-inspector": "https://github.com/<org>/ui-depth-inspector.git#v1.2.0"
```

**What release-please does NOT do:** publish to npm or any registry. UPM installs directly from the tagged git commit.

---

## 6. Supporting Files

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

Note: `trim_trailing_whitespace = false` for Markdown — trailing two spaces are a valid line-break syntax in some renderers.

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

Prevents `.meta` and `.asset` file corruption from line-ending normalization on Windows contributors.

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

### `CONTRIBUTING.md` — Structure

1. **Quick start** — fork, create a `feat/<name>` branch, open a PR against `main`.
2. **Commit rules** — Conventional Commits cheat sheet (table from Section 4), atomicity rule with example.
3. **Breaking changes** — how to signal them (`feat!` + `BREAKING CHANGE:` footer).
4. **Branch naming** — the `feat/` / `fix/` / `docs/` / `chore/` convention.
5. **PR process** — open an issue first for non-trivial features; link it in the PR via `Closes #N`.
6. **Unity version** — test against Unity 6000.0.x minimum; note the Unity version in the PR description if testing a specific patch.
7. **What NOT to do** — do not manually edit `CHANGELOG.md`; do not manually bump `version` in `package.json`.
8. **Local development setup** — install via local path override in a test project's `manifest.json`:
   ```json
   "com.openupm.ui-depth-inspector": "file:../../path/to/repo"
   ```

### `CHANGELOG.md` — Seed File

```markdown
# Changelog

All notable changes to this project will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<!-- AUTOMATICALLY MANAGED BY release-please. DO NOT EDIT MANUALLY. -->
```

release-please inserts versioned release sections above this comment on each release merge.

---

## 7. Scope Boundary — What This Does NOT Include (v1)

| Item | Status | Trigger to add |
|------|--------|----------------|
| Unity compilation check in CI | Deferred | When a self-hosted runner is available or `game-ci` licensing is sorted |
| Code of Conduct | Deferred | When community size warrants it |
| Issue templates (Bug Report, Feature Request) | Deferred | When recurring issue patterns emerge |
| CODEOWNERS | Deferred | When multiple maintainers exist |
| Pre-commit hooks (local commitlint via Husky) | Deferred | YAGNI — GitHub Actions catches violations before merge |

---

## 8. Branch Protection Rules (GitHub Repository Setting)

The commitlint CI check only blocks merges if branch protection is configured. Without this, a failing check is advisory only — anyone can merge regardless.

**Required configuration:** GitHub repo → Settings → Branches → Add rule for `main`:

| Setting | Value |
|---------|-------|
| Require status checks before merging | ✅ Enabled |
| Required status check name | `commitlint` (matches the workflow job name) |
| Require branches to be up to date before merging | ✅ Enabled |
| Do not allow bypassing the above settings | ✅ Enabled (applies to admins too) |
| Restrict who can push to matching branches | Optional — recommended: maintainer only |

This is a one-time manual step in the GitHub UI; it cannot be committed to the repo. It must be applied before accepting the first PR.

---

## 9. `.gitignore` Content

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

The package's `Editor/` folder, `package.json`, and all `.meta` files are **not** ignored — they are the deliverable. `.meta` files must be committed for UPM packages; omitting them breaks asset GUIDs for consumers.

---

## 10. `package.json` — Full UPM Manifest

All fields required or strongly recommended by the UPM specification:

```json
{
  "name": "com.<yourgithuborg>.ui-depth-inspector",
  "version": "0.1.0",
  "displayName": "UI Layer & 3D Depth Inspector",
  "description": "Interactive 3D exploded viewport for debugging uGUI Canvas draw order, raycast blockers, and mask boundaries directly inside the Unity Editor.",
  "unity": "6000.0",
  "unityRelease": "0f1",
  "documentationUrl": "https://github.com/<org>/ui-depth-inspector#readme",
  "changelogUrl": "https://github.com/<org>/ui-depth-inspector/blob/main/CHANGELOG.md",
  "licensesUrl": "https://github.com/<org>/ui-depth-inspector/blob/main/LICENSE.md",
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
    "name": "<Your Name or Org>",
    "url": "https://github.com/<org>"
  },
  "dependencies": {
    "com.unity.ugui": "2.0.0"
  }
}
```

**Field notes:**
- `unity` + `unityRelease`: UPM uses these to block installation on incompatible Unity versions. Correct values prevent support issues.
- `changelogUrl` + `licensesUrl`: rendered as clickable links in the Package Manager window.
- `keywords`: surface the package in `Window → Package Manager → Search` and on openupm.com. Keep to the most relevant terms; UPM truncates long lists.
- `dependencies`: only `com.unity.ugui`. No version wildcard — pin to the version this was built and tested against.
- `version`: do not manually edit. release-please owns this field.

---

## 11. OpenUPM Registration

OpenUPM watches GitHub release tags (created by release-please) and automatically mirrors them to its registry. No CI changes, no credentials, no publish step — registration is a one-time PR to the OpenUPM packages repo.

**Registration steps (performed once before or alongside v1.0.0 release):**

1. Fork `openupm/openupm` on GitHub.
2. Add a YAML file at `data/packages/com.<yourgithuborg>.ui-depth-inspector.yml`:
   ```yaml
   name: com.<yourgithuborg>.ui-depth-inspector
   displayName: UI Layer & 3D Depth Inspector
   description: Interactive 3D exploded viewport for debugging uGUI Canvas draw order, raycast blockers, and mask boundaries.
   repoUrl: https://github.com/<org>/ui-depth-inspector
   licenseSpdxId: MIT
   topics:
     - ui
     - ugui
     - canvas
     - debug
     - editor
   hunter: <your-github-username>
   ```
3. Open a PR. The OpenUPM bot validates the YAML and confirms the package is valid. Merge is typically within 24–48 hours.
4. After merge, OpenUPM's CI picks up existing and future GitHub release tags automatically.

**Post-registration install (what users do — one-time per project):**
```json
// manifest.json — add scoped registry once
{
  "scopedRegistries": [{
    "name": "OpenUPM",
    "url": "https://package.openupm.com",
    "scopes": ["com.<yourgithuborg>"]
  }],
  "dependencies": {
    "com.<yourgithuborg>.ui-depth-inspector": "1.0.0"
  }
}
```
Or via CLI: `openupm add com.<yourgithuborg>.ui-depth-inspector`

---

## 12. Open Questions

None. All governance decisions resolved. `<yourgithuborg>` placeholders must be replaced with the actual GitHub organization/username before the implementation plan is executed.
