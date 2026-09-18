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
| Unity compilation gate | Deferred | Over-engineered for solo maintainer; add when a self-hosted runner or second regular contributor exists. |
| Community governance (CoC, issue templates, CODEOWNERS) | Deferred | Add incrementally when community grows. |

---

## 3. Repository File Layout

```
com.openupm.ui-depth-inspector/   (repo root)
├── .editorconfig
├── .gitattributes
├── .gitignore                     # Unity + OS + IDE artifacts
├── .github/
│   ├── workflows/
│   │   ├── commitlint.yml         # PR commit validation
│   │   └── release-please.yml    # Automated CHANGELOG + version bump + GitHub Release
│   └── PULL_REQUEST_TEMPLATE.md
├── CHANGELOG.md                   # Seeded; managed by release-please
├── CONTRIBUTING.md
├── LICENSE.md                     # MIT
├── README.md
├── package.json                   # UPM manifest (version bumped by release-please)
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

## 8. Open Questions

None. All governance decisions resolved.
