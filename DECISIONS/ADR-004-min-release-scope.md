# ADR-004: Minimal release scope (historical; superseded by v1.1)

Date: 2026-10-14. Status: accepted. Freezes all out-of-scope work until the first playable release.

## Goal

One scenario with a win and a loss: grow population to **N = 50 residents** while treasury never blocks progress (spend is already blocked at 0 — bankruptcy pressure comes from paid growth vs tax income balance).

- Win: 50 residents living in the city.
- Loss: population back at 0 after day 3 (empty city = failed mandate).

## Must-have (nothing else until release)

1. Save/load, 1 slot (plain C# State serialization; see ADR-002 amendment below).
2. Goal indicator + progress (HUD: residents N/50, treasury).
3. ~~Win/lose screens.~~ Убраны решением от 15.10 (049): мешают игроку. Логика GoalSystem (Won/Lost) остаётся, экранов нет.
4. Minimal menu (new game / continue / save / quit).
5. First real player build (Windows): readiness is proven WITHOUT the Unity Editor.

## Ready criterion

New game → goal reached → save → load → goal still shown, all in the player build, Console clean.

## Frozen until release

Transport/jams, separate resources (water/power/waste), fires and services, policies/progression, disasters, more than 4–5 building types, visual polish beyond readable primitives.

## Task sketch (043+, as done)

043 save slot → 044 load slot → 045 goal HUD + win/lose → 046 minimal menu → 047 build pipeline + balance N=50 → release.

## ADR-002 review (separate item, resolved here)

Single-slot save/load ENTERS scope via this ADR and directly conflicts with ADR-002 "Persistence deferred". Resolution: ADR-002 amended — one manual slot with plain-C#-State serialization is part of the minimal release; full persistence (autosaves, slots, versioning, migration) stays deferred. No other part of ADR-002 changes.

## Post-release amendment (2026-11-07, status note)

The minimal release shipped with 047 (windows64 build + smoke), so the "Frozen until release"
clause is no longer in force. "Frozen" in the title describes the state at acceptance (2026-10-14),
not the current scope. Work listed there has since been implemented in tasks 053–102: transport
connectivity and congestion, energy/water/waste, fire and police services, tax policy and goal
stages. Win/lose screens were removed by 049; saves are at snapshot v4.

Current authoritative boundaries are `AGENTS.md` → **Scope lock**: allowed are service buildings
following the fire/police pattern (park, hospital), balance, HUD readability, bugfixes, tests and
documentation; still forbidden without a new ADR are vehicles, multi-level construction, cloud
saves, Jobs/Burst, DI containers, async/await and new third-party packages.

This note does not rewrite the original decision — it records when the freeze ended.

## Superseded by v1.1 (2026-11-08)

The v1.1 line starts from task 104. Current scope and boundaries live in `ADR-005-v1.1-scope.md`, `GAME_DESIGN.md §18` and `AGENTS.md` → Scope lock. This ADR remains as the historical record of the first minimal release.
