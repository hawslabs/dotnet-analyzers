# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/), and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased]

### Fixed

- Fixed HA9001 so split argument and parameter lists expand each item onto its own line, and fixed HA9002 so expression-bodied members with hanging parameter lists report and fix stranded `=>` tokens by keeping `) =>` on one line.
- Fixed the HA9000 code fix so hanging calls that wrap multiline raw string literals can reindent the raw string while keeping its value unchanged, including already-separated closing parens and grouped trailing parens inside the raw string.
- Fixed `just self-fix` so HA9000 code fixes run across the solution instead of only the analyzer project.
- Fixed NuGet package metadata so analyzer packing uses the MIT license expression without also declaring a license file.

### Added

- Added `CONTRIBUTING.md` for repository workflow notes and refocused the packaged README on analyzer
	installation, configuration, and usage.
- Added category-based HawsLabs diagnostic ID ranges aligned with .NET code analysis categories, including the HA9000-HA9999 Style range for the current formatting rules.
- Added `HA9005` to warn when `indent_brace_style = 1TBS` or `OTBS` conflicts with matching C# brace and newline formatting options, including per-option diagnostics for observable `csharp_prefer_braces` / `IDE0011` severity conflicts.
- Added a GitHub Actions workflow that builds, tests, packs, and publishes analyzer packages to GitHub Packages on main and repository pull requests, plus nuget.org on main.
- Added `HA9004` to format multiline raw string literal indentation, including direct return statements and raw string arguments.
- Added shared package metadata, versioning, repository, Source Link, and portable PDB settings for the analyzer NuGet package.
- Added a root `justfile` with build, verify, formatting, and focused test recipes, including watch mode, formatting checks, fix-up helpers, and name-based test filters for the analyzer workflow.
- Added manual `just self-analyze` and `just self-fix` recipes for running the analyzer package against itself.
- Added VS Code workspace tasks that delegate to the root `just` recipes, plus workspace settings to open `HawsLabs.Analyzers.slnx` by default in C# Dev Kit.
- Added `Microsoft.CodeAnalysis.PublicApiAnalyzers` to the analyzer project with baseline `PublicAPI` files.
- Added repository guidance and README updates that standardize HawsLabs analyzer-repo naming, document cross-platform `just` installation, and explain the VS Code task workflow.
- Added `HA9000`, `HA9001`, `HA9002`, and `HA9003` as the Style diagnostics reported by `HangingListClosingParenAnalyzer`.
