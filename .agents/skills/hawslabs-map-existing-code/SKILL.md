---
name: hawslabs-map-existing-code
description: Inspect the HawsLabs repository, classify current implementation against the requested feature or rule, and produce a gap map before changing code.
argument-hint: "[feature or rule]"
---
# HawsLabs Existing-Code Mapping Skill

Use this skill before implementing or refactoring HawsLabs features.

1. Read `AGENTS.md`, `.agents/copilot-instructions.md`, and relevant `.agents/instructions/*.instructions.md` files.
2. Inspect the current repository structure, analyzer packages, diagnostics, code fixes, tests, docs, and utilities.
3. Map the requested feature or rule to existing implementation.
4. Classify each item as `Implemented`, `PartiallyImplemented`, `Missing`, `Conflicting`, or `Deferred`.
5. Identify existing code to preserve and extend.
6. Identify the smallest useful implementation slice.
7. Do not edit code until the map is clear.

Output: existing implementation summary, feature classification table, code to preserve, proposed next slice, and risks or conflicts.
