# HawsLabs C# Code Analyzers

## Tooling

Install `just` with the package manager that matches your machine:

- Windows (winget): `winget install --id Casey.Just --exact`
- macOS or Linux (Homebrew): `brew install just`
- Debian 13 / Ubuntu 24.04+ derivatives: `apt install just`
- Fedora: `dnf install just`
- Arch Linux: `pacman -S just`
- Cargo fallback: `cargo install just`

## Development

This repo includes a root `justfile` as a lightweight shortcut for common .NET workflows.

- VS Code workspace tasks wrap the main `just` recipes for build, test, verify, format-check, and fix.
- `just build` — build `HawsLabs.Analyzers.slnx`
- `just test` — run the analyzer test suite
- `just test-watch` — rerun tests on change while working on analyzer code
- `just verify` — build, then run tests without rebuilding
- `just format` — run `dotnet format`
- `just format-check` — fail fast if formatting drift would make the build unhappy
- `just fix` — run formatting fixes, then build the solution
- `just self-analyze` — build the analyzer, then run it against `packages/analyzers/HawsLabs.Analyzers.csproj`
- `just self-fix` — build the analyzer, then run its HA0001 code fix against the analyzer project
- `just test-filter HangingListClosingParen` — run a focused test subset with `dotnet test --filter`
- `just test-name CodeFixTests` — run tests whose fully-qualified name contains a given value
- `just test-scope HangingListClosingParen` — preferred shorthand for scoping tests to a namespace, fixture, or feature slice
- `just test-file HangingListClosingParen` — quickly scope test runs to a fixture or folder-style namespace segment

## VS Code tasks

If you prefer staying inside VS Code, open the Command Palette and run `Tasks: Run Task`, then pick one of these workspace tasks:

- `restore HawsLabs.Analyzers.slnx`
- `build HawsLabs.Analyzers.slnx`
- `test HawsLabs.Analyzers.slnx`
- `verify HawsLabs.Analyzers.slnx`
- `format-check HawsLabs.Analyzers.slnx`
- `fix HawsLabs.Analyzers.slnx`

The workspace also pins `HawsLabs.Analyzers.slnx` as the default solution for C# Dev Kit, so opening the repo should load the intended solution automatically.

## Publishing

The `Build, test, and publish` GitHub Actions workflow restores, builds, tests, and packs `packages/analyzers/HawsLabs.Analyzers.csproj`.

- Pull requests publish prerelease packages to GitHub Packages when the PR branch belongs to this repository.
- Main branch runs publish packages to GitHub Packages and nuget.org.
- nuget.org publishing expects `NUGET_API_KEY` to be available to the workflow.
- GitHub Packages publishing uses the workflow `GITHUB_TOKEN`.
