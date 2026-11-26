# Citybuilder

3D city-building simulation in Unity 6 / C#. Current release: **v1.4.5** — «Большой город»: 100×100 grid, 1000+ residents, four road types with bridges, market/credits/districts/mayor rating, player decisions, seasons/weather, save v7; RU/EN localization (225 keys), visible systems and an endless city (no win/lose screens). Итоговые числа тестов и `bundleVersion` финализирует оркестратор на гейте #171 (`X` в документах — плейсхолдеры).

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

- **Construction & Growth:** grid 100×100 (v1.4.5, ADR-017; `Width`/`Height`/`CellSize` come only from `GridVisualizer`), configurable building cap for 1000+ residents, roads with drag-preview, R/C/I zones, tick-based building growth with 3-tick construction frames (`GrowthStageView`) and deterministic visual variants, house levels 1–3 with map markers, low-poly Hospital (`3×3`) and Park (`2×2`) FBX models from the Blender pipeline (`Tools/blender-addon/`). Old v1–v6 slots stay loadable.
- **Services & Needs v2:** unified `ServiceCoverageController` for Fire, Police, Health (Hospital) and Leisure (Park), 9 city need indicators (`NeedSystem`), satisfaction migration gate (`< 50%` stops move-in).
- **Two-Sided Economy:** hourly tax collection and building upkeep (`UpkeepPolicy`, effective ÷8), treasury deficit support, attractiveness multiplier (`0.5×..1.5×`); the HUD budget line shows taxes − upkeep − theft + rewards = total per hour.
- **Market, credits, districts and rating (v1.4.5, ADR-019):** `MarketPricePolicy` (×1 neutral by default, applied only at price points), `CreditLedger` (principal via `Treasury.Earn`, interest and repayment via `Treasury.Charge`, deficit is legal), district overrides of tax rate and market multiplier over the base `ZoneType`, and an endless mayor rating derived from milestones with no win/lose screen. The 1/2/4 tax ladder and base assets are unchanged.
- **Roads and bridges (v1.4.5, ADR-018):** four road types — `asphalt` and `brick` at level 0, `wood` and `stone` bridges at level ≥1 with `RoadCellKey(X, Y, Level)` and 0↔1 ramps. A lower road survives under a bridge; removing a bridge frees only its level.
- **Player decisions (v1.4.5, ADR-020):** `DecisionController` applies a choice on the tick boundary and writes two append-only records to the event log (offer + applied choice); pending decisions are not serialized, like fires and agents. Choice buttons live in the existing «Лента»/«Статистика» IMGUI panels.
- **Seasons and weather (v1.4.5, ADR-021):** presentation filter through `DayNightSample`/`DayNightPolicy` (six key points), emissive windows per `BlackoutViewStyle`, IMGUI effects through existing styles; money, ticks and saves are unaffected. Built-in RP only (no URP/HDRP/UI Toolkit).
- **Living city (v1.2):** deterministic fires and theft, 10 starter quests plus an endless milestone generator (3 active quests, treasury rewards), waste accumulation with garbage trucks and depot variants (Waste Depot / compost station, 6 building types on ready-made Kenney models), day/night cycle, vehicle and pedestrian agents.
- **Visibility & UX v2:** bottom action bar (`BottomBarView`), collapsible stats (`InfoMenuView`, `Tab`) with 8-day graphs and a click-to-focus minimap, event log (`EventLogView`, `L`) with RU/EN messages and sounds by event type, inspection panel with service radius overlay, road congestion heat coloring, tutorial with hints for v1.2 mechanics (fire, theft, garbage pickup, night) and a «Повторить обучение» / Replay tutorial button plus controls help, small-window layout, main and pause menus (`Esc`), RU/EN toggle in the HUD.
- **Persistence:** 3 manual save slots and autosave every 24 ticks (`SaveSlotIndex`); save v7 (ADR-022) adds optional road type/level, debt and interest, market multiplier, district tax rates, mayor rating and milestone-v2 counter, with v6 fallback and validation. Versions 1–7 are read, >7 is rejected; export/import (`SlotTransfer`) uses the same JSON and checks. Grid size is not serialized, so v1–v6 slots load unchanged. Fires, agents and pending decisions stay non-serialized (ADR-009/011/020).

## Requirements

- Unity **6000.3.25f1** (exact version in `ProjectSettings/ProjectVersion.txt`), Built-in Render Pipeline, one scene `Assets/Citybuilder/Scenes/Prototype.unity`; UI is IMGUI only.
- Git LFS (`git lfs install`) — 19 tracked objects (textures and icons). Without it those files stay pointers.
- `X` EditMode tests in `X` files (`X` `[Test]` + `X` `[TestCase]`) and `X` PlayMode tests; run them via Unity Test Runner → EditMode/PlayMode. Final counts are set by the orchestrator at the v1.4.5 gate (#171) — `X` is a placeholder, not a measured number. CI runs the required `Static validation` check on every PR, but the `Unity EditMode tests` job is skipped until repository variable `ENABLE_UNITY_TESTS=true` and Unity license secrets are set — until then EditMode is a manual gate.
- Python 3.10+ for the static validator `Tools/ValidateRepo.py`.
- Optional MCP stack: Unity bridge on `127.0.0.1:8080`, Blender 4.5 LTS MCP on `127.0.0.1:9876`.

## Quickstart

![Citybuilder v1.3.0 — HUD, нижняя панель и туториал (скриншот релиза v1.3.0)](https://github.com/albexx-dev/Citybuilder/releases/download/v1.3.0/v130-release-smoke.png)

1. Clone the repository (with Git LFS) and open **its root** as a Unity project.
2. Open `Assets/Citybuilder/Scenes/Prototype.unity` and press **Play** → **Новая игра** (New game).
3. Switch language with the **RU/EN** button in the top HUD row; the choice is persisted.
4. Follow the tutorial: paint **Зоны [3]** (Жилая / Коммерция / Промзона), enable **Рост [4]**, lay **Дороги [2]**, place buildings from **Здания [1]** (Power, Water, Waste, Compost, Fire, Police, Park, Hospital). Left-click a building in **Выкл [Esc]** mode to inspect it and see its service radius. The tutorial also reacts to fires, theft, garbage pickup and night; repeat it from **Настройки → Повторить обучение** (controls help is there too).
5. Bottom bar: **Пауза / Игра / x4** speed, **Статистика [Tab]**, **Лента [L]**, **Задачи**, **Настройки**, **Декор [5]**; **Esc** opens the Pause Menu (Save/Load slots, volume, mute). Camera: WASD, Q/E, wheel, RMB (right-drag to rotate); LMB places/inspects. **F5 / F9** save and load slot 0.
6. Static validation: `python Tools/ValidateRepo.py`.

## Download

The Windows64 build for v1.4.5 is published by the orchestrator after the green gate (#171): `Citybuilder-v1.4.5-windows64.zip` on the [GitHub Releases](https://github.com/albexx-dev/Citybuilder/releases) page. The previous released build is [v1.4.2](https://github.com/albexx-dev/Citybuilder/releases/tag/v1.4.2) (`Citybuilder-v1.4.2-windows64.zip`).

## Regenerating audio

`Tools/GenerateWav.py` (Python 3.10+, stdlib only, seed `125`) rebuilds the audio clips with fixed parameters and refuses to write inside `Assets/Citybuilder/Audio/` (the committed sources):

```bash
python Tools/GenerateWav.py --out Tools/generated-wav      # write copies
python Tools/GenerateWav.py --verify                       # SHA-256 vs committed WAVs
python Tools/GenerateWav.py --verify-exact                 # bit-exact subset only
```

Bit-for-bit identical to the committed files: `click.wav`, `coin.wav`, `pop.wav`. The other five (`swoosh.wav`, `event-fire.wav`, `event-crime.wav`, `ambient-day.wav`, `ambient-night.wav`) are deterministic but **not** byte-identical: the original per-sample formulas were never committed, only the sound descriptions in `Assets/Citybuilder/Audio/LICENSE-NOTES.md`. RIFF headers match (44 bytes, 22.05 kHz, 16-bit mono); the samples differ — see `CREDITS.md`.

## Documentation

- `ONBOARDING.md` — (RU) step-by-step setup for a new contributor, from clone to first PR.
- `CONTRIBUTING.md` — contribution rules and the pull request workflow.
- `CHANGELOG.md` — release notes for `[v1.4.5]`, `[v1.4.2]`, `[v1.4.1]`, `[v1.4.0]`, `[v1.3.3]`, `[v1.3.2]`, `[v1.3.1]`, `[v1.3.0]`, `[v1.2.2]`, `[v1.2.1]`, `[v1.2.0]`, `[v1.1.0]` and `[v1.1]`.
- `ARCHITECTURE.md` — modular monolith design and implemented systems as of v1.4.5.
- `GAME_DESIGN.md` — simulation rules, endless-game doctrine (§19) and balance (§20).
- `UNITY_MCP_SETUP.md` — Unity/Blender MCP setup and the Blender FBX exporter addon workflow.
- `DECISIONS/` — ADR-001…ADR-022 (v1.4.5 scope: ADR-017 scale, ADR-018 roads/bridges, ADR-019 market/power, ADR-020 decisions, ADR-021 visual, ADR-022 save v7; earlier: ADR-013 event localization, ADR-014 save v6).
- GitHub Issues — current task queue, bugs and proposals (templates in `.github/ISSUE_TEMPLATE/`).
