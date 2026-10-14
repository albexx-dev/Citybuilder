# ADR-001: Jobs/Burst/DOTS transition trigger

Date: 2026-10-14. Status: accepted.

Context: ARCHITECTURE §14 budgets a simulation tick at ≤5 ms and forbids Jobs/Burst/optimizations before stage 8. Population exists but is tiny (households, no per-citizen agents). The audit asked to fix the transition criterion in advance.

Decision: stay on plain C# + MonoBehaviour until Profiler measurements on a real city show EITHER `PopulationSystem.TotalResidents > 1000` with tick pressure OR simulation tick p95 > 5 ms. Only then open a task to move hot loops (ticks, employment/needs aggregates, pathfinding when it exists) to Jobs/Burst; DOTS only if Jobs/Burst is insufficient.

Consequences: no perf work without profiler numbers; `run_tests` + Play smoke stay the gates. Revisit when Transport (stage 8) lands.
