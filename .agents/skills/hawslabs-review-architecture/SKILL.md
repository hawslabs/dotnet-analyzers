---
name: hawslabs-review-architecture
description: Review HawsLabs architecture changes for repository fit, analyzer and code-fix cohesion, deterministic diagnostics, maintainability, and naming consistency.
argument-hint: "[changed area or branch]"
context: fork
---
# HawsLabs Architecture Review Skill

Review whether the change preserves existing behavior and utilities, keeps analyzer and code-fix pairing coherent, keeps diagnostics deterministic and narrowly scoped, avoids unnecessary abstractions, preserves the current package layout, keeps tests readable, and follows `HawsLabs`, `AI`, and `Id` naming.

Return findings grouped as Must fix, Should fix, Nice to have, and Verified good.
