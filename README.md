# Citybuilder

3D city-building simulation in Unity 6 / C#. Current release: **v1.3.0** — RU/EN localization (190 keys), visible systems, save v6 and an endless city (no win/lose screens).

## New here? / Новому участнику

The repository is public and self-contained — everything needed to run the game is tracked in git. The repository root **is** the Unity project (`Assets/`, `Packages/`, `ProjectSettings/`).

```bash
git clone https://github.com/albexx-dev/Citybuilder.git
cd Citybuilder
```

Open the **repository root** in Unity Hub with editor **6000.3.25f1**, then open `Assets/Citybuilder/Scenes/Prototype.unity` and press **Play**. To contribute, fork the repo and open a pull request into `main` — the required CI check is `Static validation`. GitHub owner `albexx-dev` = git author `avexx-dev` (same person, see `.github/CODEOWNERS`).

- [ONBOARDING.md](ONBOARDING.md) (RU) — full setup, verification, troubleshooting, fork → PR flow.
- [CONTRIBUTING.md](CONTRIBUTING.md) — code rules and review workflow.

Connected systems:

- **Construction & Growth:** grid 50×50, roads with drag-preview, R/C/I zones, tick-based building growth with 3-tick construction frames (`GrowthStageView`) and deterministic visual variants, house levels 1–3 with map markers, low-poly Hospital (`3×3`) and Park (`2×2`) FBX models from the Blender pipeline (`Tools/blender-addon/`).
- **Services & Needs v2:** unified `ServiceCoverageController` for Fire, Police, Health (Hospital) and Leisure (Park), 9 city need indicators (`NeedSystem`), satisfaction migration gate (`< 50%` stops move-in).
- **Two-Sided Economy:** hourly tax collection and building upkeep (`UpkeepPolicy`), treasury deficit support, attractiveness multiplier (`0.5×..1.5×`); the HUD budget line shows taxes − upkeep − theft + rewards = total per hour.
- **Living city (v1.2):** deterministic fires and theft, 10 starter quests plus an endless milestone generator (3 active quests, treasury rewards), waste accumulation with garbage trucks and depot variants (Waste Depot / compost station, 6 building types on ready-made Kenney models), day/night cycle, vehicle and pedestrian agents.
- **Visibility & UX v2:** bottom action bar (`BottomBarView`), collapsible stats (`InfoMenuView`, `Tab`) with 8-day graphs and a click-to-focus minimap, event log (`EventLogView`, `L`) with RU/EN messages and sounds by event type, inspection panel with service radius overlay, road congestion heat coloring, tutorial with hints for v1.2 mechanics (fire, theft, garbage pickup, night) and a «Повторить обучение» / Replay tutorial button plus controls help, small-window layout, main and pause menus (`Esc`), RU/EN toggle in the HUD.
- **Persistence:** 3 manual save slots and autosave every 24 ticks (`SaveSlotIndex`); save v6 adds optional garbage, theft, comfort and construction fields with validation and backward compatibility with v1–v5 slots. Fires and agents stay non-serialized (ADR-009/ADR-011).

## Requirements

- Unity **6000.3.25f1** (exact version in `ProjectSettings/ProjectVersion.txt`), Built-in Render Pipeline, one scene `Assets/Citybuilder/Scenes/Prototype.unity`; UI is IMGUI only.
- Git LFS (`git lfs install`) — 19 tracked objects (textures and icons). Without it those files stay pointers.
- 1038 EditMode tests (v1.3.0); run them via Unity Test Runner → EditMode.
- Python 3.10+ for the static validator `Tools/ValidateRepo.py`.
- Optional MCP stack: Unity bridge on `127.0.0.1:8080`, Blender 4.5 LTS MCP on `127.0.0.1:9876`.

## Quickstart

![Citybuilder v1.3.0 — HUD, нижняя панель и туториал (скриншот релиза v1.3.0)](https://github.com/albexx-dev/Citybuilder/releases/download/v1.3.0/v130-release-smoke.png)

1. Clone the repository (with Git LFS) and open **its root** as a Unity project.
2. Open `Assets/Citybuilder/Scenes/Prototype.unity` and press **Play** → **Новая игра** (New game).
3. Switch language with the **RU/EN** button in the top HUD row; the choice is persisted.
4. Follow the tutorial: paint **Зоны [3]** (Жилая / Коммерция / Промзона), enable **Рост [4]**, lay **Дороги [2]**, place buildings from **Дома [1]** (Power, Water, Waste, Compost, Fire, Police, Park, Hospital). Left-click a building in **Выкл [Esc]** mode to inspect it and see its service radius. The tutorial also reacts to fires, theft, garbage pickup and night; repeat it from **Настройки → Повторить обучение** (controls help is there too).
5. Bottom bar: **Пауза / Игра / x4** speed, **Статистика [Tab]**, **Лента [L]**, **Задачи**, **Настройки**, **Декор [5]**; **Esc** opens the Pause Menu (Save/Load slots, volume, mute). Camera: WASD, Q/E, wheel, LMB. **F5 / F9** save and load slot 0.
6. Static validation: `python Tools/ValidateRepo.py`.

## Download

Ready-to-play Windows64 build: [GitHub Release v1.3.0](https://github.com/albexx-dev/Citybuilder/releases/tag/v1.3.0) — download `Citybuilder-v1.3.0-windows64.zip`, unzip and run `CityBuilder.exe`.

## Documentation

- `ONBOARDING.md` — (RU) step-by-step setup for a new contributor, from clone to first PR.
- `CONTRIBUTING.md` — contribution rules and the pull request workflow.
- `CHANGELOG.md` — release notes for `[v1.3.0]`, `[v1.2.2]`, `[v1.2.1]`, `[v1.2.0]`, `[v1.1.0]` and `[v1.1]`.
- `ARCHITECTURE.md` — modular monolith design and implemented systems as of v1.3.
- `GAME_DESIGN.md` — simulation rules, endless-game doctrine (§19) and balance (§20).
- `UNITY_MCP_SETUP.md` — Unity/Blender MCP setup and the Blender FBX exporter addon workflow.
- `DECISIONS/` — ADR-001…ADR-014 (event localization: ADR-013, save v6: ADR-014).
- GitHub Issues — current task queue, bugs and proposals (templates in `.github/ISSUE_TEMPLATE/`).
