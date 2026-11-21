# ADR-003: Repo root is the Unity project

Date: 2026-10-14. Status: accepted (implemented in 039).

Context: the repo held only scripts; the Unity project lived outside git with manual .cs copying. origin was 26 tasks behind local — no backup.

Decision: the repo root IS the Unity project. `Assets/` (minus `Assets/Screenshots/`, local E2E artifacts), `Packages/`, `ProjectSettings/` are committed including `.meta` files. Each task ends with push of `main` to origin. Manual .cs copying is abolished; `Tools/Check-UnityMcp.ps1` checks health, not sync. Binary assets go through Git LFS (`.gitattributes` patterns pre-declared; no binaries committed yet).

Consequences: clone + open in Unity 6000.3.25f1 + Library rebuild = working project. The old folder `UnityProjects/CityBuilder` remains untouched as a frozen fallback.

## Update (2026-11-21)

The statements about binaries and the fallback in the Decision and Consequences above describe the state at the time of ADR-003 and are kept as history; they no longer hold. `.fbx` models and `.wav` sounds are committed directly in git, and binary assets matched by `.gitattributes` are stored in Git LFS — `git lfs ls-files` reports 19 objects (textures/icons). The `UnityProjects/CityBuilder` folder is neither tracked nor present in the repository root; the repo root is the only Unity project. This update supersedes those two statements; the decision itself is unchanged.
