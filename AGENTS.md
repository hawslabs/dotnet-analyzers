# AGENTS.md

These instructions apply to AI coding agents working in this repository.

## Purpose

This repository is the HawsLabs analyzer repo, not the older CodeAnalysis platform repo.

The active codebase currently centers on `packages/analyzers/HawsLabs.Analyzers.csproj`, a Roslyn analyzer/code-fix project targeting `netstandard2.0`. Current work should stay focused on HawsLabs-specific analyzer implementations, diagnostic behavior, and the surrounding repo/tooling that supports them.

## Instruction priority

- Follow the nearest applicable `AGENTS.md` if additional scoped files are added under subdirectories.
- Follow relevant files in `.agents/`, especially `copilot-instructions.md`, `.agents/instructions/*.instructions.md`, `.agents/prompts/*.prompt.md`, and `.agents/skills/*/SKILL.md`.
- When guidance conflicts, preserve working code and current repository conventions first.
- Do not remove analyzer behavior, diagnostic IDs, code-fix behavior, or repository conventions unless the user explicitly approves the removal.
- When the user corrects repository naming, scope, structure, or intended behavior, update `AGENTS.md` and the relevant `.agents` files in the same change.

## Repository layout

- `HawsLabs.Analyzers.slnx` — canonical solution for this repository.
- `packages/analyzers/` — active analyzer and code-fix implementation project.
- `justfile` — preferred shorthand for common local restore, build, test, formatting, and verification workflows.
- `.agents/` — Copilot agent, instruction, prompt, and skill customizations for this repo.
- `.github/` — GitHub configuration and workflows.
- `.vscode/` — workspace tasks, launch settings, and editor configuration.
- `.artifacts/` — generated outputs and intermediate build artifacts; avoid editing by hand.
- `Directory.Build.props`, `Directory.Packages.props`, `global.json` — shared .NET build and package settings.
- `README.md`, `CHANGELOG.md`, `TODO.md` — human-facing project documentation and backlog.

## Working conventions

- Inspect the current implementation before changing code.
- Work incrementally; do not rewrite the analyzer package from scratch.
- Keep the solution buildable after each meaningful change.
- Prefer focused changes in `packages/analyzers` over broad restructuring.
- Preserve existing utilities, public behavior, project structure, and diagnostics unless a change is necessary and explained.
- Keep documentation and customization files aligned with the actual repository scope.
- Do not edit generated or `.artifacts/` files unless the user explicitly asks.

## Naming and code conventions

- Use `HawsLabs.Analyzers` namespaces and naming for repository code unless an existing file clearly establishes a narrower pattern.
- Keep legitimate Roslyn API references such as `Microsoft.CodeAnalysis`, `Microsoft.CodeAnalysis.CSharp`, and related NuGet package names intact; they are framework dependencies, not stale repo branding.
- Use `AI`, not `Ai`.
- Use `Id` and `Ids`, not `ID` or `IDs`.
- Keep namespace folders PascalCase and project/container folders consistent with the current repository style.

## Analyzer and code-fix guidance

- Keep diagnostics deterministic, narrowly targeted, and low-noise.
- Favor early exits for missing tokens, unsupported syntax, or non-applicable cases.
- Avoid analyzing generated code.
- Keep analyzer and code-fix behavior aligned when one side changes.
- Do not change `DiagnosticId`, title, message, severity, or rule semantics unless the user explicitly requests it.
- Prefer the smallest safe syntax or text transform in code fixes.
- Preserve trivia, indentation, and existing formatting intent when fixing whitespace or layout rules.
- Use `context.CancellationToken` when accessing source text, semantic models, or other cancellable operations.

## Testing and validation

- Prefer the root `just` commands for local workflows when they cover the task: `just restore`, `just build`, `just test`, `just verify`, `just format-check`, and `just fix`.
- Build from the repository root with `dotnet build HawsLabs.Analyzers.slnx`.
- When analyzer or code-fix behavior changes, add or update focused tests.
- If the repository does not yet have the right test project for a change, add the smallest useful test project instead of inventing large test infrastructure.
- Validate with the narrowest relevant build or test commands and summarize the results.
- This repo currently has no executable sample app; do not add sample-specific tasks or launch configurations unless such a project is introduced on purpose.

## Documentation and governance

- Treat `TODO.md` as the living backlog for future ideas and intentionally deferred work.
- Keep `CHANGELOG.md` updated for notable user-visible changes using Keep a Changelog.
- Use Conventional Commits for commit messages.
- Keep `.agents` and `AGENTS.md` HawsLabs/analyzer specific; do not drift them back toward the older CodeAnalysis repository language.

## Build and validation

Run this from the repository root:

```text
just build

# or use the underlying .NET command directly
dotnet build HawsLabs.Analyzers.slnx
```
