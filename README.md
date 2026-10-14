# Citybuilder

3D city-building simulation (Unity 6, C#): connected systems — construction, roads, zones, population, employment, needs, taxes on clock ticks.

## Quickstart

1. Open the repo root `C:\Users\admin\Citybuilder` as a Unity project (Unity **6000.3.25f1**). The repo IS the project: `Assets/`, `Packages/`, `ProjectSettings/` are committed with `.meta` files.
2. Scenes: `Assets/Citybuilder/Scenes/Prototype.unity` (the game), `Assets/CityBuilder.unity` (legacy duplicate, do not use).
3. Press Play. Paint zones (mode Зоны), switch to Рост — houses grow on clock ticks, households move in, taxes collect hourly.
4. Tests: Unity Test Runner → EditMode, assembly `Tests` (Grid, Treasury, Needs). Must be green before every merge.
5. Unity bridge (MCP): editor serves `http://127.0.0.1:8080/mcp`. Health check from repo root: `powershell -ExecutionPolicy Bypass -File Tools\Check-UnityMcp.ps1`. If the bridge is stale, restart Unity (the bridge spawns with the editor).
6. Workflow: 1 task = 1 branch `task/XXX-name` = 1 local merge (`--no-ff`, orchestrator only) + push `main` to origin. Plans and queue live in `TASKS/` on the task branch, never committed to `main` directly. See `AGENTS.md`, `ARCHITECTURE.md`, `GAME_DESIGN.md`, `HANDOFF.md`.
