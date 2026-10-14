# ADR-004: Minimal release scope (frozen)

Date: 2026-10-14. Status: accepted. Freezes all out-of-scope work until the first playable release.

## Goal

One scenario with a win and a loss: grow population to **N = 50 residents** while treasury never blocks progress (spend is already blocked at 0 — bankruptcy pressure comes from paid growth vs tax income balance).

- Win: 50 residents living in the city.
- Loss: population back at 0 after day 3 (empty city = failed mandate).

## Must-have (nothing else until release)

1. Save/load, 1 slot (plain C# State serialization; see ADR-002 amendment below).
2. Goal indicator + progress (HUD: residents N/50, treasury).
3. Win/lose screens.
4. Minimal menu (new game / continue / save / quit).
5. First real player build (Windows): readiness is proven WITHOUT the Unity Editor.

## Ready criterion

New game → goal reached → save → load → goal still shown, all in the player build, Console clean.

## Frozen until release

Transport/jams, separate resources (water/power/waste), fires and services, policies/progression, disasters, more than 4–5 building types, visual polish beyond readable primitives.

## Task sketch (043+)

043 save/load slot → 044 goal HUD + win/lose → 045 minimal menu → 046 build pipeline + balance N=50 → release.

## ADR-002 review (separate item, resolved here)

Single-slot save/load ENTERS scope via this ADR and directly conflicts with ADR-002 "Persistence deferred". Resolution: ADR-002 amended — one manual slot with plain-C#-State serialization is part of the minimal release; full persistence (autosaves, slots, versioning, migration) stays deferred. No other part of ADR-002 changes.
