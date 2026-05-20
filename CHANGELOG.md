# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/), and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased]

### Fixed

- Fixed the HA0001 code fix so hanging calls that wrap multiline raw string literals can reindent the raw string while keeping its value unchanged, including already-separated closing parens and grouped trailing parens inside the raw string.
- Fixed `just self-fix` so HA0001 code fixes run across the solution instead of only the analyzer project.
- Fixed NuGet package metadata so analyzer packing uses the MIT license expression without also declaring a license file.

### Added

- Added a GitHub Actions workflow that builds, tests, packs, and publishes analyzer packages to GitHub Packages on main and repository pull requests, plus nuget.org on main.
- Added `HA0002` to format multiline raw string literal indentation, including direct return statements and raw string arguments.
- Added shared package metadata, versioning, repository, Source Link, and portable PDB settings for the analyzer NuGet package.
- Added a root `justfile` with build, verify, formatting, and focused test recipes, including watch mode, formatting checks, fix-up helpers, and name-based test filters for the analyzer workflow.
- Added manual `just self-analyze` and `just self-fix` recipes for running the analyzer package against itself.
- Added VS Code workspace tasks that delegate to the root `just` recipes, plus workspace settings to open `HawsLabs.Analyzers.slnx` by default in C# Dev Kit.
- Added `Microsoft.CodeAnalysis.PublicApiAnalyzers` to the analyzer project with baseline `PublicAPI` files.
- Added repository guidance and README updates that standardize HawsLabs analyzer-repo naming, document cross-platform `just` installation, and explain the VS Code task workflow.
- Added `HA0001` as the diagnostic ID used for `HangingListClosingParenAnalyzer` in the first release.
