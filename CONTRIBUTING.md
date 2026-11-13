# Contributing to Citybuilder

New here? Start with [ONBOARDING.md](ONBOARDING.md) (RU): cloning, the exact Unity version, first run, verification and troubleshooting. The repository is public — **fork + pull request is enough to contribute**, an invitation is optional.

## Ground rules

- The repository root **is** the Unity project. Open the root in Unity, not a subfolder.
- Use the exact editor version from `ProjectSettings/ProjectVersion.txt` (`6000.3.25f1`).
- Project code uses `namespace Citybuilder`, English identifiers, small files and small methods.
- C# file limit: 150 lines soft, 200 lines hard. Method limit: 30 lines.
- One C# file = one top-level type. Keep runtime state out of `ScriptableObject`.
- Do not add third-party packages, modify `ProjectSettings/`, or change Unity APIs without an ADR or an explicit task.
- No global singletons, `async/await`, LINQ chains, reflection, regex or DI containers in V1.
- Commit every new Unity asset together with its `.meta` file; rename and move assets inside the Unity Editor.
- Never commit generated folders (`Library/`, `Temp/`, `Logs/`, `Obj/`, `UserSettings/`, `Builds/`, `Screenshots/`) or local MCP configs.
- The MCP stack is editor tooling only: it is not required to play, build or write game code.

## Workflow (pull request — default for `main`)

`main` is protected, so the normal path is a branch plus a PR, not a direct push.

1. Sync with the base branch: `git fetch upstream && git rebase upstream/main` (fork) or `git pull --ff-only`.
2. Create a branch `task/XXX-short-name` (also fine: `fix/...`, `docs/...`).
3. For larger tasks, put the breakdown in the GitHub issue body or in the PR description.
4. Keep commits small: max 5 files and 250 changed lines.
5. Run local checks:
   - `python Tools/ValidateRepo.py` — must print `OK`; `WARN` lines about the 150-line soft limit are acceptable;
   - Unity Test Runner → **EditMode** → assembly `Tests` (434 tests) must be green;
   - `powershell -ExecutionPolicy Bypass -File Tools/Check-UnityMcp.ps1` only when the MCP stack is in use.
6. Push the branch and open a PR against `albexx-dev/Citybuilder:main`, filling in `.github/PULL_REQUEST_TEMPLATE.md`.
7. CI in `.github/workflows/ci.yml` must be green. The required check is named **`Static validation`**. The optional `Unity EditMode tests` job runs only when the repository variable `ENABLE_UNITY_TESTS=true` and the Unity license secrets are configured.
8. Review by the code owner (`.github/CODEOWNERS`), then merge through the GitHub UI. Never force-push to `main`.

Maintainers with admin rights can still merge locally with `git merge --no-ff` when CI is not gating, but the PR path is the default for everyone.

## Branch protection

`main` is the release branch. Current GitHub → Settings → Branches configuration:

- **a pull request is required before merging** — a direct push to `main` is rejected for non-admins;
- required status check: `Static validation`, strict (the PR branch must be up to date with `main`);
- force pushes and branch deletion blocked;
- review ownership comes from `.github/CODEOWNERS`;
- admins are not constrained by these rules (`enforce_admins: false`), so the owner can still push hotfixes.

These are repository settings and cannot be fully expressed as a file. If you change the CI job name, update the required check name too, otherwise PRs will stay blocked.

## Releases

Update `CHANGELOG.md`, `ProjectSettings/ProjectSettings.asset` (`bundleVersion`), create an annotated tag `vX.Y`, push the tag, then publish a GitHub release. See `DECISIONS/ADR-005-v1.1-scope.md`.

## Reporting problems

Use GitHub Issues for bugs and ideas (templates in `.github/ISSUE_TEMPLATE/`). For security issues follow `SECURITY.md` instead of opening a public issue.
