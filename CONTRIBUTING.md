# Contributing to Citybuilder

## Ground rules

- The repository root **is** the Unity project. Open the root in Unity, not a subfolder.
- Use the exact editor version from `ProjectSettings/ProjectVersion.txt` (`6000.3.25f1`).
- Project code uses `namespace Citybuilder`, English identifiers, small files and small methods.
- C# file limit: 150 lines soft, 200 lines hard. Method limit: 30 lines.
- One C# file = one top-level type. Keep runtime state out of `ScriptableObject`.
- Do not add third-party packages, modify `ProjectSettings/`, or change Unity APIs without an ADR or an explicit task.
- No global singletons, `async/await`, LINQ chains, reflection, regex or DI containers in V1.

## Workflow

1. Create a branch `task/XXX-name` from `main`.
2. Put the breakdown in `TASKS/XXX-breakdown.md`.
3. Keep commits small: max 5 files and 250 changed lines.
4. Run local checks:
   - `python Tools/ValidateRepo.py`
   - `powershell -ExecutionPolicy Bypass -File Tools/Check-UnityMcp.ps1` (only when the MCP stack is needed)
5. Run EditMode tests in Unity Test Runner, assembly `Tests` (288+ tests), before merging.
6. Merge with `git merge --no-ff`, then push `main`.
7. CI in `.github/workflows/ci.yml` must be green.

## Branch protection

`main` is the release branch. In GitHub → Settings → Branches enable:

- require the `validate` CI check;
- require branches to be up to date before merging;
- block force pushes and branch deletion.

This is a repository setting and cannot be fully expressed as a file; `.github/CODEOWNERS` adds review ownership.

## Releases

Update `CHANGELOG.md`, `ProjectSettings/ProjectSettings.asset` (`bundleVersion`), create an annotated tag `vX.Y`, push the tag, then publish a GitHub release. See `DECISIONS/ADR-005-v1.1-scope.md`.

## Reporting problems

Use GitHub Issues for bugs and ideas. For security issues follow `SECURITY.md` instead of opening a public issue.
