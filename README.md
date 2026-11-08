# Citybuilder

3D city-building simulation in Unity 6 / C#. Current line: **v1.1**.

Connected systems: construction, roads, zones, tick-based growth, population, employment, needs, taxes, energy/water/waste, fire/police coverage, road connectivity and congestion, goal stages, save/load and a HUD with onboarding hints.

## Requirements

- Unity **6000.3.25f1** — exact version in `ProjectSettings/ProjectVersion.txt`.
- Optional MCP stack: Unity bridge on `127.0.0.1:8080`, Blender MCP on `127.0.0.1:9876`.
- Python 3.10+ only for the static validator `Tools/ValidateRepo.py`.

## Quickstart

1. Clone the repository and open **its root** as a Unity project. The repo IS the project: `Assets/`, `Packages/` and `ProjectSettings/` are committed together with `.meta` files.
2. Open `Assets/Citybuilder/Scenes/Prototype.unity`.
3. Press Play. Paint zones (mode **Зоны**), switch to **Рост** — houses grow on clock ticks, households move in, taxes are collected hourly.
4. EditMode tests: Unity Test Runner → assembly `Tests` (288+ tests). They must be green before merging.
5. Static CI validation: `python Tools/ValidateRepo.py`. The Unity EditMode CI job runs only when a Unity license secret is configured.

## Configuration

- Copy `Tools/mcp.example.json` to your WorkBuddy AI MCP configuration and adjust paths/ports if needed. The real config deliberately lives outside git.
- Launcher paths are derived from the repo root or overridden with `CITYBUILDER_UNITY_EXE` / `CITYBUILDER_BLENDER_EXE`; see `UNITY_MCP_SETUP.md`.
- Health check from the repo root: `powershell -ExecutionPolicy Bypass -File Tools/Check-UnityMcp.ps1`.

## Documentation

- `CONTRIBUTING.md` — workflow, limits, CI, branch protection, releases.
- `SECURITY.md` — how to report vulnerabilities.
- `CHANGELOG.md` — version history.
- `AGENTS.md` — working rules for the orchestrator/engineer/reviewer roles.
- `ARCHITECTURE.md`, `GAME_DESIGN.md`, `DECISIONS/` — design and ADRs.
- `TASKS/README.md` — task queue. Historical `TASKS/*-breakdown.md` files are archives.
