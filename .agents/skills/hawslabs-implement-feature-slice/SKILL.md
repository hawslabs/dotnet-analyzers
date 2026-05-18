---
name: hawslabs-implement-feature-slice
description: Implement a small HawsLabs feature slice while preserving existing analyzers, code fixes, diagnostics, repo conventions, and tests.
argument-hint: "[feature slice]"
---
# HawsLabs Feature Slice Implementation Skill

1. Run the `hawslabs-map-existing-code` process for the feature slice.
2. Choose the smallest slice that can build and validate cleanly.
3. Reuse existing analyzers, code fixes, descriptors, utilities, and project structure before adding new abstractions.
4. Keep analyzer, diagnostic, and code-fix behavior aligned so the rule remains coherent end to end.
5. Prefer targeted syntax or semantic checks over broad noisy diagnostics.
6. Preserve existing HawsLabs naming, casing, and file layout.
7. Add or update focused tests when behavior changes.
8. Build and run the narrowest relevant validation.
9. Summarize what changed, what was preserved, what was validated, and what remains.
