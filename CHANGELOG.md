# Changelog

All notable changes to this project are documented here. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [v1.1] - 2026-11-08

Starting point for the v1.1 line. Built on tasks 001–104, HEAD before this change `7bf10ac`.

### Removed

- `HANDOFF.md`: obsolete handoff document. Current entry points are `README.md`, `AGENTS.md`, `TASKS/README.md`, `CHANGELOG.md` and `DECISIONS/`.
- `com.unity.multiplayer.center`: unused direct dependency removed from `Packages/manifest.json` and `Packages/packages-lock.json`.

### Changed

- Documentation aligned with the actual state after task 104: tax level is saved in `CitySnapshot.TaxLevel` and restored by `LoadController`; the ADR-004 freeze was lifted after task 047; win/lose screens were removed by task 049; `GoalSystem.Won/Lost` is explicitly retained as a basis for a later version.
- `ARCHITECTURE.md` no longer lists non-existent `GameBootstrap` / `GameState` / command-event classes; real events are `SimulationClock.TickStarted/TickCompleted` and `Treasury.BalanceChanged`.
- `GAME_DESIGN.md §18` rewritten from "frozen minimal release" to the v1.1 scope. See `DECISIONS/ADR-005-v1.1-scope.md`.
- `ProjectSettings/ProjectSettings.asset`: `bundleVersion` set to `1.1`.
- README, MCP setup docs and launcher scripts no longer hardcode `C:\Users\admin\Citybuilder`; paths are derived from the repository or environment variables.
- `SaveSlotStorage.Validate` now rejects out-of-map coordinates, invalid tax levels and invalid goal stages.
- Goal-stage rewards moved out of `GoalView.OnGUI()` into a plain C# `GoalRewardController` called from `Update()`.
- Bottom HUD lines clamp to small window heights instead of using fixed y=406/430/454 unconditionally.
- `.gitattributes` gained `* text=auto` for future line-ending normalization.

### Added

- `CONTRIBUTING.md`, `SECURITY.md`, `.github/CODEOWNERS`, `CHANGELOG.md`.
- `DECISIONS/ADR-005-v1.1-scope.md`.
- `Tools/mcp.example.json` with a portable MCP configuration template.
- `.github/workflows/ci.yml` and `Tools/ValidateRepo.py` for static CI checks; optional Unity EditMode job when a Unity license secret is configured.
- `Tests/Persistence/SaveValidationTests.cs` for the stronger snapshot validation.

### Verification baseline

- 288 existing EditMode tests in 39 files before new validation tests.
- Last documented Windows64 build: task 102, ~90 MiB, launch smoke passed.
- No PlayMode automated tests yet; gameplay scenarios are still checked manually.
