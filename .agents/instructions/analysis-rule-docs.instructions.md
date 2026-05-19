---
description: 'Use when adding or updating Roslyn analyzer or code-fix documentation in this repo, especially docs/rules.md and docs/rules/*.md. Covers the Meziantou-style rules table, per-rule markdown pages, diagnostic metadata, and code example maintenance.'
applyTo: 'docs/rules.md, docs/rules/*.md'
---

# Analyzer Rule Documentation

Maintain analyzer rule documentation in a Meziantou-style structure:

- Keep the catalog page in `docs/rules.md`.
- Keep one markdown file per rule in the lowercase `docs/rules/` folder.
- Keep full rule details in the per-rule file, not in the catalog.
- Even while the repo has only one rule, maintain both the catalog page and the per-rule page.

## Source of truth

- Derive `Id`, title, description, category, default severity, and enabled state from the analyzer's `DiagnosticDescriptor`.
- Derive code-fix availability from the code-fix provider and the corresponding tests.
- Prefer examples copied or adapted from focused analyzer/code-fix tests so the docs stay synchronized with actual behavior.
- Link the analyzer and code-fix provider source files near the top of each rule page.
- Do not document behavior, options, or automatic fixes that the code does not currently implement.
- If a requested filename, heading, or stale document disagrees with the live `DiagnosticId`, use the actual `DiagnosticId` from source instead of inventing a new one.

## Catalog page

In `docs/rules.md`:

- Start with a short heading such as `# HawsLabs.Analyzers rules`.
- Use a compact summary table inspired by `Meziantou.Analyzer`.
- Keep one row per rule, ordered by `DiagnosticId`.
- Link the `Id` cell to the matching rule page in `docs/rules/`.
- Use these columns:

| Id | Category | Description | Severity | Enabled | Code fix |
| -- | -------- | ----------- | :------: | :-----: | :------: |

- Use `ℹ️`, `⚠️`, `❌`, or `👻` for severity when a compact visual marker helps readability.
- Use `✔️` or `❌` for the `Enabled` and `Code fix` columns.
- Keep the description column short and aligned with the rule title or the analyzer's user-facing summary.

## Per-rule page template

For each `docs/rules/<DiagnosticId>.md` page, keep this section order:

1. `# <DiagnosticId> - <Rule title>`
2. Optional `Sources:` line with links to the analyzer and code-fix provider files
3. Top metadata table
4. `## Description`
5. `## Motivation`
6. `## How to fix violations`
7. `## Code with Diagnostic`
8. `## Code with Fix`
9. Optional follow-up sections only when the rule genuinely needs them, such as `## Configuration` or `## Additional resources`

Keep those section headings exact unless there is a strong repo-specific reason to change them.

Use a `Topic | Value` metadata table near the top, modeled after the `IO0004` and `IDISP014` pages:

| Topic | Value |
| :-- | :-- |
| Id | `HA0001` |
| Severity | Warning |
| Enabled | True |
| Category | Formatting |
| Code fix | Yes |
| Analyzer | `HangingListClosingParenAnalyzer` |
| Code fix provider | `HangingListClosingParenCodeFixProvider` |

Treat the sample values above as a shape example only. Always replace them with the actual values for the rule being documented.

## Writing guidance

- Write for users of the analyzer and maintainers of the repo.
- Use short, direct prose.
- In `Description`, explain exactly what the rule reports.
- In `Motivation`, explain why the rule exists: readability, consistency, correctness, or maintainability.
- In `How to fix violations`, describe the smallest compliant edit the user should make.
- When alignment or whitespace is part of the rule, explain the expected layout in plain language.
- When a rule supports multiple syntax forms, use small scenario-specific subsections under the code example sections instead of one oversized sample.

## Code samples

- Use fenced `csharp` code blocks.
- Show the non-compliant example under `## Code with Diagnostic`.
- Show the compliant result under `## Code with Fix`.
- Keep examples minimal but realistic.
- Prefer examples already covered by tests.
- Match the analyzer and code-fix behavior exactly, including whitespace and line breaks.
- If a rule has no code fix, keep the catalog row marked `❌` and replace `## Code with Fix` with a short note stating that no code fix is currently available.

## Consistency rules

- Keep the folder name lowercase: `docs/rules/`.
- Name each rule page after the real `DiagnosticId`.
- Keep page titles, metadata tables, and linked filenames consistent with the analyzer source.
- Update `docs/rules.md` and the matching per-rule page in the same change.
- When analyzer behavior, rule titles, severity defaults, or code-fix behavior changes, update the rule docs in the same PR.
- Do not leave the catalog or rule pages stale after adding a new analyzer or code fix.

## Review checklist

- [ ] `docs/rules.md` has one row per rule and links to the correct page.
- [ ] The per-rule filename matches the actual `DiagnosticId`.
- [ ] The metadata table matches the live `DiagnosticDescriptor`.
- [ ] Description, motivation, and fix guidance match current analyzer behavior.
- [ ] Diagnostic and fix samples match existing tests or verified behavior.
- [ ] Code-fix availability is correctly reflected in both the catalog and the rule page.
