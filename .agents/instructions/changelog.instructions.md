---
description: Keep CHANGELOG.md release notes human-readable and aligned with Keep a Changelog.
applyTo: "**/CHANGELOG.md"
---
- Reference: [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/) and [the changelog file guidance](https://keepachangelog.com/en/1.1.0/#what-should-the-changelog-file-be-named).
- Treat the changelog as curated release notes, not a git log.
- Keep `Unreleased` at the top.
- Move completed items into the next released version section with an ISO 8601 date (`YYYY-MM-DD`).
- Group entries under `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, and `Security`; omit empty sections.
- Describe user-visible impact in plain language and keep bullets concise.
- Mention breaking changes, removals, and deprecations explicitly.
- Keep newest releases first and preserve linkable version headings when the file uses them.
- When editing an existing entry, update it in place instead of duplicating the same change in multiple sections.
