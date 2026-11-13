# Citybuilder

3D city-building simulation in Unity 6 / C#. Current release: **v1.1.0**.

## New here? / Новому участнику

The repository is public and self-contained — everything needed to run the game is tracked in git.

```bash
git clone https://github.com/albexx-dev/Citybuilder.git
cd Citybuilder
```

Open the **repository root** in Unity Hub with editor **6000.3.25f1**, then open `Assets/Citybuilder/Scenes/Prototype.unity` and press **Play**. To contribute, fork the repo and open a pull request into `main` — the required CI check is `Static validation`.

- [ONBOARDING.md](ONBOARDING.md) (RU) — full setup, verification, troubleshooting, fork → PR flow.
- [CONTRIBUTING.md](CONTRIBUTING.md) — code rules and review workflow.

Connected systems:
- **Construction & Growth:** grid 50×50, roads with drag-preview, R/C/I zones, tick-based building growth with 3-tick construction frames (`GrowthStageView`) and deterministic visual variants, low-poly 3D FBX models for Hospital (`3×3`) and Park (`2×2`) via the Blender pipeline (`Tools/blender-addon/`).
- **Services & Needs v2:** unified `ServiceCoverageController` for Fire, Police, Health (Hospital) and Leisure (Park), 9 city need indicators (`NeedSystem`), satisfaction migration gate (`< 50%` stops move-in).
- **Two-Sided Economy:** hourly tax collection, hourly building upkeep (`UpkeepPolicy`), treasury deficit support, city attractiveness multiplier (`0.5×..1.5×`) driven by tax rate, satisfaction and services.
- **Visibility & UX v2:** bottom action bar (`BottomBarView`), collapsible statistics menu (`InfoMenuView`, `Tab`), city event log (`EventLogView`, `L`, 50-event ring buffer), building inspection panel with service radius overlay (`InfoPanelView`), road congestion heat coloring (green/yellow/red), 7-step interactive tutorial (`TutorialView`), main menu & pause menu (`Esc`) with unsaved-exit confirmation, 3 manual save slots + 24-tick autosave (`SaveSlotIndex`), and procedural UI audio (`AudioController`).

## Requirements

- Unity **6000.3.25f1** — exact version in `ProjectSettings/ProjectVersion.txt`.
- Optional MCP stack: Unity bridge on `127.0.0.1:8080`, Blender 4.5 LTS MCP on `127.0.0.1:9876`.
- Python 3.10+ for the static validator `Tools/ValidateRepo.py`.

## Quickstart

1. Clone the repository and open **its root** as a Unity project (`Assets/`, `Packages/`, and `ProjectSettings/` are tracked with `.meta` files).
2. Open `Assets/Citybuilder/Scenes/Prototype.unity` and press **Play**.
3. Click **Новая игра** in the main menu. Follow the 7-step tutorial at the top of the screen:
   - Paint **Зоны [3]** (Жилая / Коммерция / Промзона) and enable **Рост [4]**.
   - Build roads (**Дороги [2]**) and utility/service buildings (**Дома [1]** palette: Power, Water, Waste, Fire, Police, Park, Hospital).
   - Inspect any building by left-clicking it in **Выкл [Esc]** mode to see its status and service radius.
   - Open **Статистика [Tab]** or **Лента [L]** from the bottom bar, or press **Esc** for the Pause Menu (Save/Load slots, volume slider, mute toggle).
4. Static validation: `python Tools/ValidateRepo.py`.

## Documentation

- `ONBOARDING.md` — (RU) step-by-step setup for a new contributor, from clone to first PR.
- `CONTRIBUTING.md` — contribution rules and the pull request workflow.
- `CHANGELOG.md` — full release notes for `[v1.1.0]` and `[v1.1]`.
- `ARCHITECTURE.md` — modular monolith design and implemented v1.1.0 systems.
- `GAME_DESIGN.md` — simulation rules, infinite-game doctrine (§19), and v1.1 balance table (§20).
- `UNITY_MCP_SETUP.md` — Unity/Blender MCP setup and the Blender FBX exporter addon workflow.
- `DECISIONS/` — architecture decision records (ADR-005 and later).
- GitHub Issues — current task queue, bugs and proposals (templates in `.github/ISSUE_TEMPLATE/`).
