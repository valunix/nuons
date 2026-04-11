# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What is Nuons

Nuons is a .NET source generator library that eliminates boilerplate for dependency injection, constructor injection, and HTTP endpoint registration. Users mark code with attributes; Roslyn source generators produce the wiring code at compile time. It ships as two NuGet packages: `Nuons` (for all projects) and `Nuons.Startup` (for the entry-point project only).

## Build and Test Commands

```bash
# Build the main solution
dotnet build ./Nuons.slnx

# Run all unit tests (uses Microsoft.Testing.Platform runner)
dotnet test --solution Nuons.slnx

# Run a single test project
dotnet test tests/Nuons.DependencyInjection.Generators.Tests

# Run a single test by name
dotnet test tests/Nuons.DependencyInjection.Generators.Tests --filter "ServiceRegistrationsAreGeneratedCorrectly"

# Build local NuGet packages for end-to-end testing (from build/ directory)
pwsh ./build/build-end-to-end-test-nuget.ps1
# Then uncomment the local-feed source in nuget.config and use NuonsEndToEnd.slnx
```

Target framework: .NET 10. Source generator projects target `netstandard2.0` (Roslyn requirement).

## Architecture

### Project Naming Convention

`Nuons.{Area}.{ProjectType}` where:

- **Area**: `Core`, `DependencyInjection`, `CodeInjection`, `Http`
- **ProjectType**: `Abstractions` (marker attributes for client code), `Generators` (assembly-scoped source generators), `Generators.Startup` (cross-assembly generators for startup projects), `Analyzers` (diagnostic analyzers)

### Two-Phase Generator Pattern

Startup-dependent features (like DI service registration) use two generators:

1. **Assembly-scoped generator** (`Generators`) - runs per-assembly, emits predefined intermediate code
2. **Startup generator** (`Generators.Startup`) - runs only in the startup project, scans referenced assemblies marked with `[AssemblyHasNuons]`, and combines intermediate outputs into final registration code

This is the core architectural pattern. Understand it before modifying any generator.

### Packaging

`Nuons.csproj` and `Nuons.Startup.csproj` are packaging-only projects. They reference all other projects and manually specify which DLLs go into `lib/netstandard2.0/` (abstractions, for client use) vs `analyzers/dotnet/cs/` (generators + analyzers, for Roslyn).

### Generator Implementation Pattern

Each generator follows this structure:

- `*Generator.cs` - implements `IIncrementalGenerator`, uses `ForAttributeWithMetadataName` where possible
- `*Increment.cs` - data model for the incremental pipeline
- `*SourceBuilder.cs` - builds the generated C# source string
- `Known*Types.cs` - string constants for fully qualified type names (avoids hardcoded strings)

Generators reference attribute types via constants from `Known*Types` classes, never hardcoded strings.

### Test Infrastructure

Generator tests use **Verify** (snapshot testing) with xUnit v3:

- `Nuons.Core.Tests` provides shared fixtures: `NuonGeneratorFixture` (compiles source, runs generator, captures output) and `NuonAnalyzerFixture`
- Each test project has a `Samples.cs` with input source code and `FixtureExtensions.cs` that wire up the correct assembly markers and test context
- Verified snapshots are `*.verified.txt` files alongside tests
- Tests with `[Fact(Skip = "For debugging during dev only")]` are intentional dev-time helpers

### Code Style

- Tabs for indentation (4-width), LF line endings
- File-scoped namespaces
- Generated code goes in the `Nuons` namespace to avoid conflicts with user code
- Generators should always produce output even if empty (easier debugging, supports chaining)

## Documentation

- Documenation is kept as md files in `documentation` directory
- Use it when you need more context for implementing features and update it when making relevant changes
