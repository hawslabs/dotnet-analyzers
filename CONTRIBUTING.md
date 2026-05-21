# Contributing

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
- `just build` builds `HawsLabs.Analyzers.slnx`.
- `just test` runs the analyzer test suite.
- `just test-watch` reruns tests on change while working on analyzer code.
- `just verify` builds, then runs tests without rebuilding.
- `just format` runs `dotnet format`.
- `just format-check` fails fast if formatting drift would make the build unhappy.
- `just fix` runs formatting fixes, then builds the solution.
- `just self-analyze` builds the analyzer, then runs it against `packages/analyzers/HawsLabs.Analyzers.csproj`.
- `just self-fix` builds the analyzer, then runs its formatting code fixes against the solution.
- `just test-filter HangingListClosingParen` runs a focused test subset with `dotnet test --filter`.
- `just test-name CodeFixTests` runs tests whose fully-qualified name contains a given value.
- `just test-scope HangingListClosingParen` scopes tests to a namespace, fixture, or feature slice.
- `just test-file HangingListClosingParen` scopes test runs to a fixture or folder-style namespace segment.

## VS Code Tasks

If you prefer staying inside VS Code, open the Command Palette and run `Tasks: Run Task`, then pick
one of these workspace tasks:

- `restore HawsLabs.Analyzers.slnx`
- `build HawsLabs.Analyzers.slnx`
- `test HawsLabs.Analyzers.slnx`
- `verify HawsLabs.Analyzers.slnx`
- `format-check HawsLabs.Analyzers.slnx`
- `fix HawsLabs.Analyzers.slnx`

The workspace also pins `HawsLabs.Analyzers.slnx` as the default solution for C# Dev Kit, so opening
the repo should load the intended solution automatically.

## Publishing

The `Build, test, and publish` GitHub Actions workflow restores, builds, tests, and packs
`packages/analyzers/HawsLabs.Analyzers.csproj`.

- Pull requests publish prerelease packages to GitHub Packages when the PR branch belongs to this repository.
- Main branch runs publish packages to GitHub Packages and nuget.org.
- nuget.org publishing expects `NUGET_API_KEY` to be available to the workflow.
- GitHub Packages publishing uses the workflow `GITHUB_TOKEN`.