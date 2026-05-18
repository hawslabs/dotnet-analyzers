---
description: Write MADR ADRs with consistent filenames, structure, and decision history.
applyTo: "**/docs/adr/**/*.md"
---
- Reference: [MADR overview](https://adr.github.io/madr/) and [MADR full template](https://adr.github.io/madr/#full-template).
- Store each architecturally significant decision in its own markdown file under `docs/adr`.
- Name files `NNNN-title-with-dashes.md` with a zero-padded sequence and a lowercase hyphenated title.
- Use one decision per file; do not combine unrelated decisions.
- Follow the MADR structure: title, context and problem statement, decision drivers, considered options, decision outcome, consequences, and confirmation when useful.
- Prefer concise rationale over implementation detail.
- Add subfolders only when they reflect the architecture or product structure.
- When a decision changes materially, update the existing ADR and mark the outcome clearly instead of creating a duplicate record.
