---
name: add-generator
description: 'Scaffold a new Roslyn source generator following the Nuons project pattern (Generator + Increment + SourceBuilder + KnownTypes + Abstractions attribute + tests).'
---

# Add Generator

Scaffold a new source generator in this codebase following the project's established pattern.

## When to use

Use this skill when the user wants to add a new source generator that emits code based on a marker attribute. Examples:

- "Add a generator for `[Decorator]`"
- "Create a new generator that produces X for classes marked with Y"
- "Scaffold a generator in the Http area"

If the user just wants a new attribute (no generator) or a new analyzer, do NOT use this skill — those are different patterns.

## Inputs to gather

Ask the user (or infer from their request) before generating files:

1. **Feature name** — PascalCase, e.g. `Decorator`, `EventHandler`, `Validator`. Used for class/file names.
2. **Area** — one of `DependencyInjection`, `CodeInjection`, `Http`. If a new area is needed, stop and ask the user to confirm — creating a new area requires changes to packaging projects (`Nuons.csproj`, `Nuons.Startup.csproj`) and is out of scope for this skill.
3. **Attribute name** — typically `{Feature}Attribute` (e.g. `DecoratorAttribute`).
4. **Attribute target** — `Class`, `Method`, `Field`, etc. Defaults to `Class`.
5. **Attribute constructor parameters** — what data the attribute captures (zero or more parameters).
6. **What the generator emits** — a one-line description so `SourceBuilder` template comments make sense.

If any of 1–5 are unclear, ask once with a compact question — don't ask multiple back-and-forth rounds.

## Project pattern reference

Every generator in this repo has these four files in `src/Nuons.{Area}.Generators/`:

- `{Feature}.cs` — `internal record` holding extracted data
- `{Feature}Increment.cs` — `internal record` with `string? AssemblyName` + `ImmutableArray<{Feature}>`
- `{Feature}SourceBuilder.cs` — builds the generated C# string
- `{Feature}Generator.cs` — `[Generator]` class implementing `IIncrementalGenerator`

Plus a constant added to `Known{Area}Types.cs` (the file already exists per area; do not create a new one).

Plus the marker attribute in `src/Nuons.{Area}.Abstractions/{Feature}Attribute.cs`.

Plus tests in `tests/Nuons.{Area}.Generators.Tests/`:

- `{Feature}GeneratorTests.cs` — Verify-based snapshot test
- A new sample type appended to existing `Samples.cs`

The canonical reference for the simplest complete generator is `OptionsRegistration*` in [src/Nuons.DependencyInjection.Generators/](src/Nuons.DependencyInjection.Generators/) — read those four files before scaffolding if the pattern needs refreshing.

## Steps to execute

Execute these in order. Use the `Read` tool to inspect existing files before editing them; use `Write` for new files and `Edit` for additions.

### 1. Verify the area exists

Confirm `src/Nuons.{Area}.Abstractions/`, `src/Nuons.{Area}.Generators/`, and `tests/Nuons.{Area}.Generators.Tests/` all exist. If any are missing, stop and report — area scaffolding is out of scope.

### 2. Create the marker attribute

Path: `src/Nuons.{Area}.Abstractions/{Feature}Attribute.cs`

Template (adjust target, ctor params, and XML doc to inputs):

```csharp
using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.{Area}.Abstractions;

/// <summary>
/// {One-line description of what marking a {Target} with this attribute does.}
/// </summary>
[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.{Target}, AllowMultiple = false, Inherited = false)]
public class {Feature}Attribute : Attribute
{
	/// <summary>Constructor.</summary>
	public {Feature}Attribute({CtorParams}) { }
}
```

The `[Conditional(Constants.CodeGenerationCondition)]` line is mandatory — it strips the attribute from runtime IL since it is only meaningful at compile time.

### 3. Add the known type constant

Path: `src/Nuons.{Area}.Generators/Known{Area}Types.cs`

Add a new constant line. Read the file first to match formatting (tabs, alphabetical or appended — match existing convention):

```csharp
public const string {Feature}Attribute = "Nuons.{Area}.Abstractions.{Feature}Attribute";
```

### 4. Create the data record

Path: `src/Nuons.{Area}.Generators/{Feature}.cs`

```csharp
namespace Nuons.{Area}.Generators;

internal record {Feature}({DataFields});
```

`DataFields` should be the strongly-typed shape of what the generator extracts from an attribute usage. At minimum, include the target's fully qualified type name.

### 5. Create the increment record

Path: `src/Nuons.{Area}.Generators/{Feature}Increment.cs`

```csharp
using System.Collections.Immutable;

namespace Nuons.{Area}.Generators;

internal record {Feature}Increment(string? AssemblyName, ImmutableArray<{Feature}> Items);
```

Naming: use a plural collection name that fits the feature (`Registrations`, `Endpoints`, `Items`, etc.). Match the style of existing increments in the same area.

### 6. Create the source builder

Path: `src/Nuons.{Area}.Generators/{Feature}SourceBuilder.cs`

```csharp
using System.Text;
using Nuons.Core.Generators;

namespace Nuons.{Area}.Generators;

internal class {Feature}SourceBuilder
{
	private readonly string className;
	private readonly List<string> entries = [];

	public {Feature}SourceBuilder(string className)
	{
		this.className = className;
	}

	public void With{Feature}({Feature} item)
	{
		// TODO: build the per-item generated line
		entries.Add($"{Sources.Tab2}// {item}");
	}

	public string Build()
	{
		var builder = new StringBuilder();
		entries.ForEach(entry =>
		{
			builder.AppendLine();
			builder.Append(entry);
		});

		return $@"namespace Nuons.{Area}.Extensions;

public static class {{className}}
{{{{
	public static void Apply()
	{{{{{{builder}}}
	}}}}
}}}}";
	}
}
```

Note: the `$@"..."` raw template is the most fiddly piece — verify brace escaping (`{{` for literal `{`) is correct after generation. Compare against `OptionsRegistrationSourceBuilder.cs` for the exact escaping pattern.

### 7. Create the generator

Path: `src/Nuons.{Area}.Generators/{Feature}Generator.cs`

```csharp
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nuons.Core.Generators;

namespace Nuons.{Area}.Generators;

[Generator]
internal class {Feature}Generator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var assemblyNameProvider = context.CompilationProvider
			.Select((compilation, _) => compilation.AssemblyName);

		var itemsProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			Known{Area}Types.{Feature}Attribute,
			Syntax.Is{Target}Node,
			Extract
		)
			.WhereNotNull()
			.Collect();

		var incrementProvider = assemblyNameProvider.Combine(itemsProvider)
			.Select((combined, _) => new {Feature}Increment(combined.Left, combined.Right));

		context.RegisterSourceOutput(incrementProvider, GenerateSources);
	}

	private {Feature}? Extract(GeneratorAttributeSyntaxContext context, CancellationToken token)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
		{
			return null;
		}

		// TODO: read attribute ctor args and build the {Feature} record
		return new {Feature}(symbol.ToFullTypeName());
	}

	private void GenerateSources(SourceProductionContext context, {Feature}Increment increment)
	{
		if (increment.AssemblyName is null)
		{
			return;
		}

		var className = $"{Feature}{Sources.TrimForClassName(increment.AssemblyName)}";
		var builder = new {Feature}SourceBuilder(className);
		foreach (var item in increment.Items)
		{
			builder.With{Feature}(item);
		}

		var sourceText = SourceText.From(builder.Build(), Encoding.UTF8);
		context.AddSource(Sources.GeneratedNameHint(className), sourceText);
	}
}
```

Notes:
- `Syntax.Is{Target}Node` — use `IsClassNode` for class targets. If targeting methods/fields, check `Syntax.cs` in `Nuons.Core.Generators` for the available predicate or add a new one.
- `WhereNotNull()` and `ToFullTypeName()` are extension methods provided by `Nuons.Core.Generators` — they're available via the `using Nuons.Core.Generators;` import.
- The generator MUST always produce output even when empty — that's the project rule. The current template returns early when `AssemblyName is null`, which is correct (only happens for malformed compilations); it does NOT return early when `Items` is empty, so the empty-output case is naturally handled by the `foreach` not executing.

### 8. Create the test class

Path: `tests/Nuons.{Area}.Generators.Tests/{Feature}GeneratorTests.cs`

```csharp
using Nuons.Core.Tests;

namespace Nuons.{Area}.Generators.Tests;

public class {Feature}GeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<{Feature}Generator>(output);

	[Fact]
	public Task {Feature}IsGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<{Feature}Generator>();
		return Verify(sources);
	}
}
```

### 9. Append a sample to Samples.cs

Path: `tests/Nuons.{Area}.Generators.Tests/Samples.cs`

Read the file, then append a sample type that uses the new attribute. Match the existing style (file-scoped namespace, `internal partial class` if the generator emits partial-class extensions, or `internal class` otherwise).

Example addition:

```csharp
[{Feature}({SampleArgs})]
internal class Sample{Feature}Target;
```

### 10. Build and run the test

Run `dotnet build ./Nuons.slnx` to verify everything compiles. Then run:

```bash
dotnet test tests/Nuons.{Area}.Generators.Tests --filter "{Feature}IsGeneratedCorrectly"
```

The first run will fail because no `.verified.txt` exists yet — Verify will create a `.received.txt` next to the test file. Tell the user:

> The test produced `{Feature}GeneratorTests.{Feature}IsGeneratedCorrectly.received.txt`. Review it; if it matches what you want the generator to emit, rename it to `.verified.txt` (or use your Verify diff tool to accept it).

## Two-phase startup-scoped variant

If the user says the generator needs to combine outputs across assemblies (like `ServiceRegistration` does via `CombinedServiceRegistration`), the single-phase template above is NOT enough. Stop and tell the user:

> This generator needs the two-phase startup pattern. That requires a separate `Nuons.{Area}.Generators.Startup` project plus packaging changes. Want me to scaffold that as a follow-up step? Reference: `src/Nuons.DependencyInjection.Generators.Startup/`.

Do not attempt the two-phase variant inside this skill.

## Validation checklist

Before reporting the task as complete:

- [ ] All 7 new files created with the correct namespaces (`Nuons.{Area}.Abstractions` for the attribute, `Nuons.{Area}.Generators` for the four generator files, `Nuons.{Area}.Generators.Tests` for the test).
- [ ] `Known{Area}Types.cs` updated with the new constant.
- [ ] `Samples.cs` updated with a sample.
- [ ] `dotnet build ./Nuons.slnx` passes.
- [ ] Test runs (failing on first run with a `.received.txt` is expected and OK).
- [ ] All files use TAB indentation, LF line endings, and file-scoped namespaces (project conventions from `CLAUDE.md`).
