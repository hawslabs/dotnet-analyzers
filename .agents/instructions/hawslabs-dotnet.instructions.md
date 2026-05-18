---
applyTo: "**/*.cs,**/*.csproj,**/*.props,**/*.targets,**/*.slnx,Directory.Build.*"
---
# HawsLabs .NET Instructions

- Use normal .NET naming, formatting, nullable reference types, DI, options, logging, and cancellation patterns.
- Use `HawsLabs.*` root namespaces and PascalCase namespace folders inside C# projects.
- Use `Id` casing for identifiers.
- Avoid bare `Id` properties. Use typed identifiers and explicit property names such as `TenantId`, `WorkspaceId`, `PlanId`, `SourceSnapshotId`, and `PipelineRunId`.
- Prefer immutable records for messages and simple value objects.
- Prefer interfaces only when there is a real boundary, test seam, provider, or host-specific implementation.
- Use `readonly record struct` or small value objects for typed identifiers when practical.
- Keep public API changes incremental and update callers/tests in the same change.

## Cancellation Tokens
- Use `CancellationToken` parameters in async methods that perform I/O, network, storage, analysis, or long-running work.
- Name the parameter `ct` for brevity and consistency.
- Pass `ct` through to all async calls and operations that support cancellation.
- Don't set `= default` for `CancellationToken` parameters in public APIs. Require callers to explicitly pass a token or `CancellationToken.None`.
- Name async methods without the `Async` suffix - if there is a sync and async version, use the Sync suffix for the sync version and no suffix for the async version.
- Check `ct.IsCancellationRequested` periodically in long-running loops or operations and throw `OperationCanceledException` if cancellation is requested.
- Avoid using `CancellationToken.None` or ignoring cancellation tokens in async methods that support cancellation.
- Use `ct.ThrowIfCancellationRequested()` to check for cancellation at the beginning of async methods or before starting long-running work.