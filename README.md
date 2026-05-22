# HawsLabs C# Code Analyzers

HawsLabs.Analyzers is a Roslyn analyzer package with C# diagnostics and code fixes for HawsLabs
formatting conventions.

## Install

Install the package into each project that should be analyzed:

```bash
dotnet add package HawsLabs.Analyzers
```

Keep analyzer references private so they do not flow to downstream consumers of your project:

```xml
<ItemGroup>
	<PackageReference Include="HawsLabs.Analyzers" Version="x.y.z" PrivateAssets="all" />
</ItemGroup>
```

For projects that use Central Package Management, put the version in `Directory.Packages.props`:

```xml
<ItemGroup>
	<PackageVersion Include="HawsLabs.Analyzers" Version="x.y.z" />
</ItemGroup>
```

Then reference the analyzer from each project:

```xml
<ItemGroup>
	<PackageReference Include="HawsLabs.Analyzers" PrivateAssets="all" />
</ItemGroup>
```

## Configure

Configure diagnostics in `.editorconfig` with standard Roslyn analyzer severity settings:

```editorconfig
[*.cs]
dotnet_diagnostic.HA9000.severity = warning
dotnet_diagnostic.HA9001.severity = warning
dotnet_diagnostic.HA9002.severity = warning
dotnet_diagnostic.HA9003.severity = warning
dotnet_diagnostic.HA9004.severity = warning
dotnet_diagnostic.HA9005.severity = warning
dotnet_diagnostic.HA9006.severity = warning
```

Use `error` to fail builds for a rule, or `none` to disable a rule.

Several formatting rules also read existing EditorConfig settings:

```editorconfig
[*]
max_line_length = 110
indent_brace_style = 1TBS

[*.cs]
csharp_prefer_braces = true:error
csharp_new_line_before_open_brace = none
csharp_new_line_before_else = false
csharp_new_line_before_catch = false
csharp_new_line_before_finally = false
csharp_new_line_before_members_in_object_initializers = false
csharp_new_line_before_members_in_anonymous_types = false
csharp_new_line_between_query_expression_clauses = false
dotnet_diagnostic.IDE0011.severity = error
```

## Use

After restore, the analyzers run anywhere Roslyn analyzers run: Visual Studio, VS Code with C# Dev Kit,
`dotnet build`, and CI builds.

Apply supported code fixes from your editor, or run them from the command line:

```bash
dotnet format <solution-or-project> analyzers --diagnostics HA9000 HA9001 HA9002 HA9003 HA9004 HA9006
```

## Rules

### Rule ID ranges

| Range       | Category         |
| ----------- | ---------------- |
| HA1000-1199 | Design           |
| HA1200-1299 | Documentation    |
| HA1300-1399 | Globalization    |
| HA1400-1499 | Interoperability |
| HA1500-1699 | Maintainability  |
| HA1700-1799 | Naming           |
| HA1800-1999 | Performance      |
| HA2000-2099 | Reliability      |
| HA2100-2199 | Security         |
| HA2200-2299 | Usage            |
| HA2300-2399 | SingleFile       |
| HA9000-9999 | Style            |

### Rule catalog

| Id                             | Category | Description                                                    | Severity | Enabled | Code fix |
| ------------------------------ | -------- | -------------------------------------------------------------- | :------: | :-----: | :------: |
| [HA9000](docs/rules/HA9000.md) | Style    | Put hanging-list closing parenthesis on its own line           |    ⚠️     |    ✔️    |    ✔️     |
| [HA9001](docs/rules/HA9001.md) | Style    | Put split list items on separate lines                         |    ⚠️     |    ✔️    |    ✔️     |
| [HA9002](docs/rules/HA9002.md) | Style    | Keep parameter-list continuations with the closing parenthesis |    ⚠️     |    ✔️    |    ✔️     |
| [HA9003](docs/rules/HA9003.md) | Style    | Keep short First calls with their receiver                     |    ⚠️     |    ✔️    |    ✔️     |
| [HA9004](docs/rules/HA9004.md) | Style    | Format multiline raw string literal indentation                |    ⚠️     |    ✔️    |    ✔️     |
| [HA9005](docs/rules/HA9005.md) | Style    | Keep 1TBS brace settings consistent                            |    ⚠️     |    ✔️    |    ❌     |
| [HA9006](docs/rules/HA9006.md) | Style    | Align hanging-list items with continuation indentation         |    ⚠️     |    ✔️    |    ✔️     |

## Contributing

Contributor setup, development commands, VS Code tasks, and publishing notes live in
[CONTRIBUTING.md](https://github.com/HawsLabs/dotnet-analyzers/blob/main/CONTRIBUTING.md).
