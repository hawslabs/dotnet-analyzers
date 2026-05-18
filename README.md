# HawsLabs C# Code Analyzers

## Development

This repo includes a root `justfile` as a lightweight shortcut for common .NET workflows.

- `just build` — build `HawsLabs.Analyzers.slnx`
- `just test` — run the analyzer test suite
- `just test-watch` — rerun tests on change while working on analyzer code
- `just verify` — build, then run tests without rebuilding
- `just format` — run `dotnet format`
- `just format-check` — fail fast if formatting drift would make the build unhappy
- `just fix` — run formatting fixes, then build the solution
- `just test-filter HangingListClosingParen` — run a focused test subset with `dotnet test --filter`
- `just test-name CodeFixTests` — run tests whose fully-qualified name contains a given value
- `just test-scope HangingListClosingParen` — preferred shorthand for scoping tests to a namespace, fixture, or feature slice
- `just test-file HangingListClosingParen` — quickly scope test runs to a fixture or folder-style namespace segment
