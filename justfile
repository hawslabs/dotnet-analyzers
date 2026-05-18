set windows-shell := ["powershell.exe", "-NoLogo", "-NoProfile", "-Command"]

default:
    @just --list
restore:
    dotnet restore HawsLabs.Analyzers.slnx
build:
    dotnet build HawsLabs.Analyzers.slnx
test:
    dotnet test HawsLabs.Analyzers.slnx
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
    dotnet format HawsLabs.Analyzers.slnx
format-check:
    dotnet format HawsLabs.Analyzers.slnx --verify-no-changes
fix:
    dotnet format HawsLabs.Analyzers.slnx
    dotnet build HawsLabs.Analyzers.slnx
clean:
    dotnet clean HawsLabs.Analyzers.slnx
verify:
    dotnet build HawsLabs.Analyzers.slnx
    dotnet test HawsLabs.Analyzers.slnx --no-build
