# ADR-002: Persistence deferred, state discipline locked

Date: 2026-10-14. Status: amended by ADR-004 (single-slot save/load enters minimal-release scope; full persistence still deferred).

Context: audit P1 — runtime state lives in MonoBehaviour memory and does not survive domain reload; full save/load is not V1.

Decision: full Persistence stays out of scope until a dedicated task. Until then: all MonoBehaviour state is lazy-rebuilt after domain reload (Ensure*/OnEnable-resubscribe pattern); new systems MUST keep runtime state in plain C# classes separate from View (ARCHITECTURE §4/§12); no new system may hold state only in MonoBehaviour fields.

Consequences: reload wipes the grown city (accepted, documented in every handoff); future save/load serializes plain C# State without presentation refactor.
