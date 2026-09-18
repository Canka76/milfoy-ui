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
