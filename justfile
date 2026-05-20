set shell := ["bash", "-euo", "pipefail", "-c"]
set windows-shell := ["powershell.exe", "-NoLogo", "-NoProfile", "-Command"]

solution := "HawsLabs.Analyzers.slnx"
analyzer_project := "packages/analyzers/HawsLabs.Analyzers.csproj"
self_artifacts_path := justfile_directory() / ".artifacts" / "self-analyze"
self_analyzer_path := self_artifacts_path / "bin" / "HawsLabs.Analyzers" / "debug" / "HawsLabs.Analyzers.dll"
ci_configuration := env_var_or_default("BUILD_CONFIGURATION", "Release")
package_output_path := env_var_or_default("PACKAGE_OUTPUT_PATH", ".artifacts/packages")
test_results_path := env_var_or_default("TEST_RESULTS_PATH", ".artifacts/test-results")

default:
    @just --list

restore:
    dotnet restore {{ solution }}

build:
    dotnet build {{ solution }}

test:
    dotnet test {{ solution }}

test-watch:
    dotnet watch test tests/HawsLabs.Analyzers.Tests/HawsLabs.Analyzers.Tests.csproj

test-filter filter:
    dotnet test tests/HawsLabs.Analyzers.Tests/HawsLabs.Analyzers.Tests.csproj --filter '{{ filter }}'

test-name name:
    dotnet test tests/HawsLabs.Analyzers.Tests/HawsLabs.Analyzers.Tests.csproj --filter 'FullyQualifiedName~{{ name }}'

test-scope scope:
    dotnet test tests/HawsLabs.Analyzers.Tests/HawsLabs.Analyzers.Tests.csproj --filter 'FullyQualifiedName~{{ scope }}'

test-file path:
    dotnet test tests/HawsLabs.Analyzers.Tests/HawsLabs.Analyzers.Tests.csproj --filter 'FullyQualifiedName~{{ path }}'

format:
    dotnet format {{ solution }}

format-check:
    dotnet format {{ solution }} --verify-no-changes

fix:
    dotnet format {{ solution }}
    dotnet build {{ solution }}

self-analyze:
    dotnet build {{ analyzer_project }} -p:ArtifactsPath={{ self_artifacts_path }}
    dotnet build {{ analyzer_project }} --no-restore -p:ArtifactsPath={{ self_artifacts_path }} -p:RunSelfAnalyzer=true

self-fix:
    dotnet build {{ analyzer_project }} -p:ArtifactsPath={{ self_artifacts_path }}
    dotnet restore {{ solution }}
    $env:RunSelfAnalyzer = 'true'; $env:SelfAnalyzerPath = '{{ self_analyzer_path }}'; dotnet format {{ solution }} analyzers --diagnostics HA0001 HA0002 --severity warn --no-restore

clean:
    dotnet clean {{ solution }}

verify:
    dotnet build {{ solution }}
    dotnet test {{ solution }} --no-build

ci: ci-restore ci-build ci-test ci-pack

ci-restore:
    dotnet restore {{ solution }} --locked-mode

ci-build:
    dotnet build {{ solution }} --configuration {{ ci_configuration }} --no-restore

ci-test:
    dotnet test {{ solution }} --configuration {{ ci_configuration }} --no-build --logger trx --results-directory {{ test_results_path }}

ci-pack:
    dotnet pack {{ analyzer_project }} --configuration {{ ci_configuration }} --no-build --output {{ package_output_path }}
