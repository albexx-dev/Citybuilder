# Changelog

All notable changes to this project are documented here. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [v1.1.0] - 2026-11-09

Complete release of the **v1.1** product milestone (Epics #1–#6, #8–#21).

### Added

- **Bottom Action Bar & Info Menu (EPIC-13, EPIC-14):** Replaced legacy top-left button stacks with a unified bottom bar (`BottomBarView`, `BottomBarStyle`, `HudLayout`) and a collapsible statistics window (`InfoMenuView`, toggled via `Статистика` or `Tab`).
- **Generalized Services v2 & Hospital / Park (EPIC-01, EPIC-02, ADR-006):** Unified service coverage domain (`ServiceSource`, `ServiceBuildingSystem`, `ServiceCoverageController`) supporting `Fire`, `Police`, `Health`, and `Leisure`, plus `Hospital.asset` (`3×3`, radius 7) and `Park.asset` (`2×2`, radius 5).
- **Needs v2 (EPIC-03):** Added `HealthNeedProvider` and `LeisureNeedProvider` (6 providers total), expanded `NeedSystem` to 9 indicators, and gated household move-in when overall satisfaction drops below `50%`.
- **Building Upkeep & City Attractiveness (EPIC-04, EPIC-05):** Added `UpkeepPerHour` to `BuildingDefinition`, `UpkeepPolicy`, `UpkeepStatusPolicy`, `UpkeepView`, negative treasury deficit support (`Treasury.Charge`/`IsInDeficit`), and `AttractivenessPolicy` (`0.5×..1.5×` migration interval multiplier shown in `InfoMenuView` and `GrowthStatusView`).
- **City Event Log (EPIC-09, ADR-007):** Added `CityEventLog` (ring buffer of 50 events with 10-tick deduplication), `EventLogView` (`Лента` button or `L`), and 4 event sources (supply deficit, road congestion, treasury deficit, high unemployment).
- **Building Inspection Panel & Radius Overlay (EPIC-12):** Added `BuildingInfoQuery`, `InfoPanelView`, and `GridVisualizer.ShowRadiusOverlay` to inspect any grown or manual building and highlight its service coverage radius on the grid.
- **Visible Road Congestion (EPIC-15):** Added `CongestionSystem.Load01`, per-tick road heat recoloring in `RoadView` (green `< 0.33`, yellow `0.33..0.66`, red `≥ 0.66`), and congestion summary line in `InfoMenuView`.
- **Saves v2 — 3 Slots & Autosave (EPIC-11, ADR-008):** Added `SaveSlotIndex` (`slots.json`), 3 manual save slots (`slot_0..2.json`), `AutosaveController` (every 24 ticks into `autosave.json`), `SaveMenuView`, and `LoadMenuView`.
- **Main Menu & Pause Menu (EPIC-16):** Reworked `MainMenu` as a centered overlay (`Продолжить`, `Новая игра`, `Загрузить`, `Выход`) and added `PauseMenuView` (`Esc`) with unsaved-changes exit confirmation.
- **Building Growth Stages & Variants (EPIC-17):** Added 3-tick construction scaffolding (`GrowthStageView`) with smooth scale interpolation and 3 deterministic visual variants for houses (`BuildingDefinition.SelectVariant`).
- **Interactive Tutorial v2 (EPIC-18):** Replaced `StartHintPolicy`/`StartHintView` with a 7-step `TutorialPolicy` and `TutorialView` featuring active button highlights and a `Понятно` dismiss button.
- **Blender Pipeline & 3D Models (EPIC-20):** Added `Tools/blender-addon/citybuilder_export.py` and `generate_models.py`, exported deterministic low-poly `hospital.fbx` and `park.fbx` models into `Assets/Citybuilder/Models/`, and wired them to `Hospital.asset` and `Park.asset`.
- **UI Audio Subsystem (EPIC-19):** Added `AudioController`, 4 procedural CC0 WAV clips (`click.wav`, `coin.wav`, `pop.wav`, `swoosh.wav`), and volume/mute controls backed by `PlayerPrefs` in `PauseMenuView`.

### Changed

- **Balance Pass (EPIC-06):** Tuned all 10 building definitions in `Assets/Citybuilder/Data/Buildings/`, raised `MaxSettlements` from 30 to 80 so the 200-resident milestone is reachable, and documented the full balance table in `GAME_DESIGN.md §20`.
- **File Splits (EPIC-10):** Extracted `RoadDragInput` from `RoadPlacementController` and removed unused `Assets/Citybuilder/Scripts/Editor/` folder so all 135+ C# scripts stay strictly under the 200-line hard limit (`ValidateRepo.py` warnings reduced to 10).

### Removed

- **Win/Lose Outcomes (EPIC-08):** Removed `Won`, `Lost`, `LoseDay`, and `LoseTicks` from `GoalSystem` in accordance with the endless-game doctrine (`GAME_DESIGN.md §19`).

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
