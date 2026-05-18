# HawsLabs Repository Instructions

Follow these instructions for all GitHub Copilot work in this HawsLabs repository.

- Preserve existing analyzers, code fixes, diagnostics, and repository structure unless the user explicitly approves removal.
- Work incrementally against the existing codebase; do not rewrite the analyzer package from scratch.
- Prefer focused changes in `packages/analyzers` over broad restructuring.
- Use `HawsLabs` for project, namespace, and product naming in this repository.
- Use `AI`, not `Ai`.
- Use `Id` and `Ids`, not `ID` or `IDs`.
- Keep analyzer, diagnostic descriptor, and code-fix behavior aligned when one side changes.
- Keep diagnostics deterministic, narrowly targeted, and low-noise.
- Preserve current formatting and style intent unless the requested change explicitly broadens scope.
- Add or update focused tests when analyzer or code-fix behavior changes.
- Run the narrowest relevant validation and summarize what changed, what was preserved, and any remaining gaps.
