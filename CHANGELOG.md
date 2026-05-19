# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/), and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased]

### Added

- Added a root `justfile` with build, verify, formatting, and focused test recipes, including watch mode, formatting checks, fix-up helpers, and name-based test filters for the analyzer workflow.
- Added VS Code workspace tasks that delegate to the root `just` recipes, plus workspace settings to open `HawsLabs.Analyzers.slnx` by default in C# Dev Kit.
- Added `Microsoft.CodeAnalysis.PublicApiAnalyzers` to the analyzer project with baseline `PublicAPI` files.
- Added repository guidance and README updates that standardize HawsLabs analyzer-repo naming, document cross-platform `just` installation, and explain the VS Code task workflow.
- Added `HA0001` as the diagnostic ID used for `HangingListClosingParenAnalyzer` in the first release.
