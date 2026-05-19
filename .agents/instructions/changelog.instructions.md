---
description: Keep CHANGELOG.md release notes human-readable, aligned with Keep a Changelog, and free of duplicate unreleased entries.
applyTo: "**/CHANGELOG.md"
---
- Reference: [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/) and [the changelog file guidance](https://keepachangelog.com/en/1.1.0/#what-should-the-changelog-file-be-named).
- Treat the changelog as curated release notes, not a git log.
- Keep `Unreleased` at the top.
- Treat `Unreleased` as the single draft for the next release. If work is still unreleased, rewrite the original bullet in place instead of adding a second bullet that says the unreleased item changed.
- Keep pending work in its original category unless the category itself is wrong. If it needs reclassification before release, move the original bullet instead of duplicating it.
- Use `Changed` only for behavior or user-visible output that changed relative to the latest released version.
- If the project has no released versions yet, do not add a `Changed` section under `Unreleased`.
- Move completed items into the next released version section with an ISO 8601 date (`YYYY-MM-DD`).
- Group entries under `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, and `Security`; omit empty sections.
- Describe user-visible impact in plain language and keep bullets concise.
- Mention breaking changes, removals, and deprecations explicitly.
- Keep newest releases first and preserve linkable version headings when the file uses them.
- When editing an existing entry, update it in place instead of duplicating the same change in multiple sections.
