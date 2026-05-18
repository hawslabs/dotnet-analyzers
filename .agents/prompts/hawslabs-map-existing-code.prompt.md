---
name: hawslabs-map-existing-code
description: Map the current implementation against a plan section before editing.
agent: agent
argument-hint: "[plan section or feature]"
---

Read `AGENTS.md`, `.agents/copilot-instructions.md`, and the relevant instruction files. Inspect the repository and classify the requested feature as `Implemented`, `PartiallyImplemented`, `Missing`, `Conflicting`, or `Deferred`. Do not change code. Return a concise gap map, existing code to preserve, and the smallest useful next implementation slice.
