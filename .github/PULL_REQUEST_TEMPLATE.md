## What changed

<!-- Short, factual description. One PR = one task. -->

## Why

<!-- Link the issue: Closes #NN -->
Closes #

## How it was verified

<!-- Which commands / tests were run and what the result was. -->

## Checklist

- [ ] `python Tools/ValidateRepo.py` passes locally (no `ERROR` lines)
- [ ] Unity **6000.3.25f1**, Test Runner → EditMode → assembly `Tests` (434 tests) is green
- [ ] New or renamed Unity assets are committed together with their `.meta` files
- [ ] No `Library/`, `Temp/`, `Logs/`, `Obj/`, `UserSettings/`, `Builds/`, `Screenshots/` or local configs are in the diff
- [ ] No secrets, personal absolute paths or machine-specific settings are committed
- [ ] `ProjectSettings/` and `Packages/manifest.json` are untouched, or the change is backed by an ADR / explicit task
- [ ] Docs updated (`README.md`, `CHANGELOG.md`, `ARCHITECTURE.md`, `GAME_DESIGN.md`) if behaviour changed
- [ ] `Static validation` CI check is green and the branch is up to date with `main`
