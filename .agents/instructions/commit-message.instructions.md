---
description: Generate concise Conventional Commit messages for repository changes.
---
- Use Conventional Commits: `<type>(<scope>): <imperative summary>`.
- Prefer `feat`, `fix`, `docs`, `refactor`, `test`, `build`, `ci`, `chore`, `perf`, or `style`.
- Use a focused scope when it clarifies the touched area, such as `analyzers`, `code-fix`, `tests`, `vscode`, or `agents`.
- Keep the summary short, imperative, and specific to the changed behavior.
- Add a body after a blank line only when extra context helps reviewers understand the reason for the change.
- Add `BREAKING CHANGE: ...` in the footer for incompatible changes.
- Emit the final commit message only; avoid comments, placeholders, markdown fences, or explanatory text.
