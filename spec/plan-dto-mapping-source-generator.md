# Implementation Plan: DTO Mapping Source Generator

**Spec**: [spec-design-dto-mapping-source-generator.md](spec-design-dto-mapping-source-generator.md)
**Date**: 2026-03-24
**Status**: Not Started

---

## Instructions for Executing Model

This plan is divided into sequential steps. Each step has a clear goal, specific files to create or modify, acceptance criteria, and a verification command. **You MUST complete each step fully and verify it before proceeding.**

**After completing each step:**

1. Run the verification command(s) listed
1. Confirm all acceptance criteria are met
1. **STOP and ask the user: "Step N is complete. Shall I continue with Step N+1?"**

**Important rules:**

- Read the referenced spec file and any referenced existing files BEFORE writing code
- Follow existing code patterns exactly — do not invent new conventions
- All generated type references must use `global::` prefix
- All attributes must use `[Conditional(Constants.CodeGenerationCondition)]`
- Target `netstandard2.0` for abstractions and generator projects
- Use `IIncrementalGenerator` with `ForAttributeWithMetadataName`
- When in doubt, look at how `InjectConstructorGenerator` does it and follow that pattern

---

## Step 1: Add Attributes to Abstractions Project

**Goal**: Create `MapFromAttribute` and `MapIgnoreAttribute` in the `Nuons.CodeInjection.Abstractions` project, following the exact same pattern as existing attributes (`InjectConstructorAttribute`, `InjectedAttribute`).

**Files to read first:**

- `src/Nuons.CodeInjection.Abstractions/InjectConstructorAttribute.cs` — pattern to follow
- `src/Nuons.Core.Abstractions/Constants.cs` — for `Constants.CodeGenerationCondition`

**Files to create:**

### 1a. `src/Nuons.CodeInjection.Abstractions/MapFromAttribute.cs`

```csharp
using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.CodeInjection.Abstractions;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class MapFromAttribute : Attribute
{
    public MapFromAttribute(Type sourceType) { }
}
```

### 1b. `src/Nuons.CodeInjection.Abstractions/MapIgnoreAttribute.cs`

```csharp
using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.CodeInjection.Abstractions;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class MapIgnoreAttribute : Attribute
{
    public MapIgnoreAttribute(string propertyName) { }
}
```

**Key details:**

- `MapFromAttribute`: `AllowMultiple = false` — one source type per DTO
- `MapIgnoreAttribute`: `AllowMultiple = true` — can exclude multiple properties
- Both use `[Conditional(Constants.CodeGenerationCondition)]` for erasure
- Both target `AttributeTargets.Class` (records are classes in Roslyn)
- Constructor parameters are positional only — no need to store them as properties (the generator reads them from the semantic model)

**Acceptance criteria:**

- [ ] Both files exist in `src/Nuons.CodeInjection.Abstractions/`
- [ ] Both files compile without errors
- [ ] Both use `[Conditional(Constants.CodeGenerationCondition)]`
- [ ] Namespaces match: `Nuons.CodeInjection.Abstractions`

**Verification:**

```bash
dotnet build src/Nuons.CodeInjection.Abstractions/
```

**STOP**: Ask the user "Step 1 is complete. Shall I continue with Step 2?"

---

## Step 2: Register Known Types and Create Data Models

**Goal**: Add FQN constants for the new attributes to `KnownCodeInjectionTypes` and create the increment data model records that the generator pipeline will use.

**Files to read first:**

- `src/Nuons.CodeInjection.Generators/KnownCodeInjectionTypes.cs` — add new constants here
- `src/Nuons.CodeInjection.Generators/InjectionIncrement.cs` — pattern for increment record
- `src/Nuons.CodeInjection.Generators/InjectedField.cs` — pattern for property record

**Files to modify:**

### 2a. `src/Nuons.CodeInjection.Generators/KnownCodeInjectionTypes.cs`

Add two new constants to the existing class:

```csharp
public const string MapFromAttribute = "Nuons.CodeInjection.Abstractions.MapFromAttribute";
public const string MapIgnoreAttribute = "Nuons.CodeInjection.Abstractions.MapIgnoreAttribute";
```

**Files to create:**

### 2b. `src/Nuons.CodeInjection.Generators/MappingIncrement.cs`

```csharp
using System.Collections.Immutable;

namespace Nuons.CodeInjection.Generators;

internal record MappingIncrement(
    string TargetNamespace,
    string TargetRecordName,
    string TargetAccessibility,
    string SourceFullTypeName,
    string SourceSimpleName,
    ImmutableArray<MappedProperty> Properties
);
```

### 2c. `src/Nuons.CodeInjection.Generators/MappedProperty.cs`

```csharp
namespace Nuons.CodeInjection.Generators;

internal record MappedProperty(
    string Name,
    string FullTypeName
);
```

**Key details:**

- Use `ImmutableArray<MappedProperty>` (not `IReadOnlyList`) to match the pattern from `InjectionIncrement` which uses `ImmutableArray<InjectedField>`. Immutable arrays are required for incremental generator caching/equality.
- `SourceFullTypeName` includes `global::` prefix (e.g., `global::MyApp.Domain.Order`)
- `SourceSimpleName` is the short name (e.g., `Order`) — used only if needed for readability
- `TargetAccessibility` is lowercase: `"public"` or `"internal"`

**Acceptance criteria:**

- [ ] `KnownCodeInjectionTypes` has both new constants with correct FQNs
- [ ] `MappingIncrement.cs` and `MappedProperty.cs` exist in the generators project
- [ ] Project compiles without errors

**Verification:**

```bash
dotnet build src/Nuons.CodeInjection.Generators/
```

**STOP**: Ask the user "Step 2 is complete. Shall I continue with Step 3?"

---

## Step 3: Implement the Source Builders

**Goal**: Create two source builder classes — one for the partial record (properties) and one for the extension method (mapping). Follow the `InjectionSourceBuilder` pattern.

**Files to read first:**

- `src/Nuons.CodeInjection.Generators/InjectionSourceBuilder.cs` — the pattern to follow
- `src/Nuons.Core.Generators/Sources.cs` — for `Sources.Tab1`, `Sources.Tab2`, `Sources.NewLine`

**Files to create:**

### 3a. `src/Nuons.CodeInjection.Generators/MappingRecordSourceBuilder.cs`

This builder generates the partial record file with cloned properties.

**Expected output shape:**

```csharp
namespace MyApp.Contracts;
public partial record OrderDto
{
    public global::System.Int32 Id { get; init; }
    public global::System.String CustomerName { get; init; }
}
```

**Implementation guidance:**

- Constructor takes: `string namespaceName`, `string recordName`, `string accessibility`
- `With(MappedProperty property)` method adds a property line to an internal list
- `Build()` method assembles the full source string
- Use `Sources.Tab1` for indentation (single tab, matching existing pattern)
- Properties use `{ get; init; }` (REQ-012)
- Use string interpolation and `Aggregate` for joining, consistent with `InjectionSourceBuilder.Build()`

### 3b. `src/Nuons.CodeInjection.Generators/MappingExtensionSourceBuilder.cs`

This builder generates the static extension method class.

**Expected output shape:**

```csharp
namespace MyApp.Contracts;
public static class OrderDtoExtensions
{
    public static OrderDto ToOrderDto(this global::MyApp.Domain.Order source)
    {
        return new OrderDto
        {
            Id = source.Id,
            CustomerName = source.CustomerName,
        };
    }
}
```

**Implementation guidance:**

- Constructor takes: `string namespaceName`, `string recordName`, `string accessibility`, `string sourceFullTypeName`
- `With(MappedProperty property)` method adds a property assignment to an internal list
- `Build()` method assembles the full source string
- Extension method name: `To{RecordName}` (REQ-006)
- Extension class name: `{RecordName}Extensions`
- Parameter name is always `source`
- Each assignment: `{Name} = source.{Name},` (trailing comma is fine)
- Use `Sources.Tab1`, `Sources.Tab2` for indentation
- Handle edge case: if no properties, generate `return new RecordName {};` (empty initializer)

**Key details:**

- Both builders are `internal` classes
- Both follow the With/Build pattern from `InjectionSourceBuilder`
- Use tab indentation (not spaces) — this matches the existing generated output
- The extension method class accessibility should match the target record's accessibility

**Acceptance criteria:**

- [ ] Both source builder files exist in the generators project
- [ ] Both follow the With/Build pattern
- [ ] Project compiles without errors
- [ ] Builders handle the empty properties edge case

**Verification:**

```bash
dotnet build src/Nuons.CodeInjection.Generators/
```

**STOP**: Ask the user "Step 3 is complete. Shall I continue with Step 4?"

---

## Step 4: Implement the Generator

**Goal**: Create `DtoMappingGenerator` implementing `IIncrementalGenerator`. This is the core of the feature. Follow the `InjectConstructorGenerator` pattern exactly.

**Files to read first:**

- `src/Nuons.CodeInjection.Generators/InjectConstructorGenerator.cs` — the pattern to follow exactly
- `src/Nuons.Core.Generators/Syntax.cs` — for helper methods (`ToFullTypeName`, `ToNamespaceSimple`, `IsClassNode`)

**File to create:**

### 4a. `src/Nuons.CodeInjection.Generators/DtoMappingGenerator.cs`

**Structure — three methods:**

#### `Initialize` — Pipeline setup

```csharp
[Generator]
internal class DtoMappingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            KnownCodeInjectionTypes.MapFromAttribute,
            Syntax.IsClassNode,
            ExtractMappingIncrement
        ).Where(increment => increment is not null);

        context.RegisterSourceOutput(provider, GenerateSources!);
    }
}
```

#### `ExtractMappingIncrement` — Transform step

This is the most complex method. It must:

1. **Validate the target is a partial record** (REQ-007):
   - Check `symbol` is `INamedTypeSymbol`
   - Check `symbol.IsRecord` is `true`
   - Check the syntax declaration has `partial` modifier
   - If not a partial record, return `null` (silently skip per AC-008)

1. **Extract target metadata**:
   - Namespace: `symbol.ToNamespaceSimple()`
   - Record name: `symbol.Name`
   - Accessibility: `symbol.DeclaredAccessibility` switch (`Public` -> `"public"`, else `"internal"`)

1. **Extract source type from `[MapFrom(typeof(T))]`**:
   - Get the attribute data from `context.Attributes` (it's the first one since `AllowMultiple = false`)
   - Read the constructor argument: `attributeData.ConstructorArguments[0].Value as INamedTypeSymbol`
   - If null, return `null`
   - `SourceFullTypeName`: `sourceType.ToFullTypeName()` (gives `global::` prefixed name)
   - `SourceSimpleName`: `sourceType.Name`

1. **Collect `[MapIgnore]` property names**:
   - Iterate ALL attributes on the symbol (not just the first)
   - Filter for attributes whose class name is `"MapIgnoreAttribute"` (use `KnownCodeInjectionTypes.MapIgnoreAttribute` or match by attribute class full name)
   - For each, read `constructorArguments[0].Value as string`
   - Collect into a `HashSet<string>` for O(1) lookup

1. **Extract and filter source properties**:
   - Get all members of the source type: `sourceType.GetMembers().OfType<IPropertySymbol>()`
   - Filter: `property.DeclaredAccessibility == Accessibility.Public`
   - Filter: `property.GetMethod is not null` (must have a getter)
   - Filter: property name is NOT in the MapIgnore set
   - Filter: property type is in the supported set (see below)
   - For each passing property: create `MappedProperty(property.Name, property.Type.ToFullTypeName())`
   - Collect into `ImmutableArray<MappedProperty>`

1. **Supported type check** (REQ-010, REQ-011):

   Create a helper method `IsSupportedType(ITypeSymbol type)` that returns `true` for:
   - If the type is `INamedTypeSymbol namedType` and `namedType.OriginalDefinition.SpecialType` is `Nullable<T>`, recursively check the type argument
   - `SpecialType` values: `Boolean`, `Byte`, `SByte`, `Int16`, `UInt16`, `Int32`, `UInt32`, `Int64`, `UInt64`, `Single`, `Double`, `Decimal`, `Char`, `String`
   - `TypeKind.Enum`
   - For `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`, `Guid`: check by full metadata name (e.g., `type.ToDisplayString() == "System.DateTime"` or match against `type.ContainingNamespace` + `type.Name`)

   **Important**: For nullable value types like `int?`, the `ToFullTypeName` with `ExpandNullable` will produce `global::System.Nullable<global::System.Int32>`. The existing `Syntax.ToFullTypeName` already has `ExpandNullable` in its format options, which handles this correctly.

   **For nullable reference types** like `string?`, `ToFullTypeName` already includes `IncludeNullableReferenceTypeModifier`, so it will produce `global::System.String?`.

1. **Return** the `MappingIncrement` record, or `null` if any validation failed.

#### `GenerateSources` — Output step

This method produces **two** source outputs per increment:

```csharp
private void GenerateSources(SourceProductionContext context, MappingIncrement increment)
{
    // File 1: Partial record with properties
    var recordBuilder = new MappingRecordSourceBuilder(
        increment.TargetNamespace, increment.TargetRecordName, increment.TargetAccessibility);
    foreach (var property in increment.Properties)
        recordBuilder.With(property);
    var recordSource = SourceText.From(recordBuilder.Build(), Encoding.UTF8);
    context.AddSource(Sources.GeneratedNameHint(increment.TargetRecordName), recordSource);

    // File 2: Extension method class
    var extensionBuilder = new MappingExtensionSourceBuilder(
        increment.TargetNamespace, increment.TargetRecordName,
        increment.TargetAccessibility, increment.SourceFullTypeName);
    foreach (var property in increment.Properties)
        extensionBuilder.With(property);
    var extensionSource = SourceText.From(extensionBuilder.Build(), Encoding.UTF8);
    context.AddSource(Sources.GeneratedNameHint($"{increment.TargetRecordName}Extensions"), extensionSource);
}
```

**Key details:**

- The `[Generator]` attribute is required on the class
- The class must be `internal` (matching `InjectConstructorGenerator`)
- Use `Encoding.UTF8` when creating `SourceText`
- Two calls to `context.AddSource` — one per generated file
- Name hints: `"{RecordName}.g.cs"` and `"{RecordName}Extensions.g.cs"` (via `Sources.GeneratedNameHint`)

**Acceptance criteria:**

- [ ] `DtoMappingGenerator.cs` exists in the generators project
- [ ] It implements `IIncrementalGenerator`
- [ ] It uses `ForAttributeWithMetadataName` with the correct FQN
- [ ] It validates partial record requirement
- [ ] It correctly reads `MapFrom` type argument from semantic model
- [ ] It correctly reads `MapIgnore` property names
- [ ] It filters properties by supported types
- [ ] It produces two source outputs
- [ ] Project compiles without errors

**Verification:**

```bash
dotnet build src/Nuons.CodeInjection.Generators/
```

**STOP**: Ask the user "Step 4 is complete. Shall I continue with Step 5?"

---

## Step 5: Create Test Samples and Fixture Extensions

**Goal**: Create the shared domain types (reference source), individual test input files (one per test case), and wire up the test fixture for the new generator. Each test case gets its own sample file and snapshot.

**Files to read first:**

- `tests/Nuons.CodeInjection.Generators.Tests/Samples.cs` — existing sample pattern
- `tests/Nuons.CodeInjection.Generators.Tests/FixtureExtensions.cs` — how the fixture is wired
- `tests/Nuons.Core.Tests/NuonGeneratorTestContext.cs` — note the `ReferencesSourcePath` parameter
- `tests/Nuons.Core.Tests/NuonGeneratorFixture.cs` — understand how `ReferencesSourcePath` is used: it creates a separate compilation that is emitted and added as a metadata reference to the main compilation. This lets domain types live in one file and DTOs in separate files.

**Architecture for test separation:**

- **Shared domain types** go into `MappingSamplesShared.cs` — this file is used as `ReferencesSourcePath`
- **Each test case** gets its own small input file containing ONLY the annotated DTO record(s) for that scenario — these are used as `InputSourcePath`
- This way each test produces its own snapshot, making failures easy to diagnose

**Files to create:**

### 5a. `tests/Nuons.CodeInjection.Generators.Tests/MappingSamplesShared.cs`

This file contains ALL domain (source) types shared across tests. It is compiled as a **reference assembly**, NOT as generator input.

```csharp
namespace Nuons.CodeInjection.Generators.Tests;

// Enum for testing enum property mapping
public enum SampleOrderStatus
{
    Pending,
    Shipped,
    Delivered
}

// Standard domain type — all properties are supported types
public class SampleOrder
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public SampleOrderStatus Status { get; set; }
    public Guid InternalTrackingId { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
}

// Domain type with nullable properties
public class SampleProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int? DiscountPercent { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? LastModified { get; set; }
}

// Domain type with mixed supported/unsupported properties
public class SampleComplexEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];          // unsupported — List<T>
    public SampleOrder? RelatedOrder { get; set; }         // unsupported — nested object
    public SampleOrderStatus Status { get; set; }          // supported — enum
    public DateTime CreatedAt { get; set; }                // supported
}

// Domain type with read-only properties
public class SampleReadOnlyEntity
{
    public int Id { get; }
    public string Name { get; } = string.Empty;
}
```

**Important**: This file does NOT have `using Nuons.CodeInjection.Abstractions;` — domain types must not reference Nuons attributes (CON-005).

### 5b. Individual test input files

Create one file per test case. Each file contains ONLY the DTO record(s) needed for that specific test. All files use the same namespace.

**`tests/Nuons.CodeInjection.Generators.Tests/MappingSamples/AllPublicPropertiesAreMapped.cs`**

```csharp
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

[MapFrom(typeof(SampleOrder))]
public partial record SampleOrderDto;
```

**`tests/Nuons.CodeInjection.Generators.Tests/MappingSamples/MapIgnoreExcludesProperties.cs`**

```csharp
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

[MapFrom(typeof(SampleOrder))]
[MapIgnore("InternalTrackingId")]
public partial record SampleOrderWithIgnoreDto;
```

**`tests/Nuons.CodeInjection.Generators.Tests/MappingSamples/NullablePropertiesArePreserved.cs`**

```csharp
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

[MapFrom(typeof(SampleProduct))]
public partial record SampleProductDto;
```

**`tests/Nuons.CodeInjection.Generators.Tests/MappingSamples/EnumPropertiesAreMapped.cs`**

```csharp
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

// Reuses SampleOrder which has SampleOrderStatus enum property
[MapFrom(typeof(SampleOrder))]
public partial record SampleOrderEnumDto;
```

**`tests/Nuons.CodeInjection.Generators.Tests/MappingSamples/UnsupportedTypesAreSkipped.cs`**

```csharp
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

[MapFrom(typeof(SampleComplexEntity))]
public partial record SampleComplexEntityDto;
```

**`tests/Nuons.CodeInjection.Generators.Tests/MappingSamples/ReadOnlySourcePropertiesAreMapped.cs`**

```csharp
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

[MapFrom(typeof(SampleReadOnlyEntity))]
public partial record SampleReadOnlyEntityDto;
```

**`tests/Nuons.CodeInjection.Generators.Tests/MappingSamples/ExtensionMethodIsGeneratedCorrectly.cs`**

```csharp
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

// Uses SampleOrder — the extension method is the focus of this test's snapshot
[MapFrom(typeof(SampleOrder))]
public partial record SampleOrderExtDto;
```

**`tests/Nuons.CodeInjection.Generators.Tests/MappingSamples/NonPartialRecordIsSkipped.cs`**

```csharp
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

// Non-partial record — the generator should produce NO output
[MapFrom(typeof(SampleOrder))]
public record SampleNonPartialDto;
```

**Note on the non-partial record file**: The `[Conditional]` attribute erasure only affects compiled IL output, NOT the Roslyn syntax/semantic model during source generation. The test fixture creates a `CSharpCompilation` from raw source text, so the `[MapFrom]` attribute WILL be visible to the generator during the test. This means the generator's partial-record check (REQ-007) is genuinely tested. The generator should return `null` from `ExtractMappingIncrement`, producing empty output. The test verifies this by asserting the output is `string.Empty`.

### 5c. Modify `tests/Nuons.CodeInjection.Generators.Tests/FixtureExtensions.cs`

Read the existing file first. Add new extension methods for the mapping generator. These methods use the `ReferencesSourcePath` parameter to pass the shared domain types as a reference compilation.

```csharp
// Add these to the existing static class:

private const string MappingSamplesSharedPath = "../../../MappingSamplesShared.cs";
private const string MappingSamplesDir = "../../../MappingSamples";

private static NuonGeneratorTestContext MappingContext(string sampleFileName) =>
    new($"{MappingSamplesDir}/{sampleFileName}", AssemblyMarkers, MappingSamplesSharedPath);

public static string GenerateMappingSources(this NuonGeneratorFixture fixture, string sampleFileName) =>
    fixture.GenerateSources<DtoMappingGenerator>(MappingContext(sampleFileName));

public static void RunMappingGenerator(this NuonGeneratorFixture fixture, string sampleFileName, ITestOutputHelper output) =>
    fixture.RunGenerator<DtoMappingGenerator>(MappingContext(sampleFileName), output);
```

**Key details:**

- `MappingSamplesSharedPath` points to the shared domain types file — used as `ReferencesSourcePath`
- `MappingSamplesDir` points to the folder with individual test input files
- `MappingContext(sampleFileName)` creates a `NuonGeneratorTestContext` per test case
- The fixture's `Drive` method will compile `MappingSamplesShared.cs` into a reference assembly, then compile the individual sample file against it + the reference assembly
- Reuses the existing `AssemblyMarkers` array (includes `CodeInjectionAbstractionsAssemblyMarker`)

**Acceptance criteria:**

- [ ] `MappingSamplesShared.cs` exists with all domain types (no Nuons attribute imports)
- [ ] `MappingSamples/` folder exists with 8 individual test input files
- [ ] Each input file contains only the DTO record(s) for that scenario
- [ ] `FixtureExtensions.cs` has new methods accepting `sampleFileName` parameter
- [ ] Test project compiles without errors

**Verification:**

```bash
dotnet build tests/Nuons.CodeInjection.Generators.Tests/
```

**STOP**: Ask the user "Step 5 is complete. Shall I continue with Step 6?"

---

## Step 6: Create Generator Tests and Verify Snapshots

**Goal**: Create the test class with one `[Fact]` per test case from the spec (Section 6), each producing its own snapshot. Run them and accept the snapshots.

**Files to read first:**

- `tests/Nuons.CodeInjection.Generators.Tests/InjectConstructorGeneratorTests.cs` — test pattern to follow

**File to create:**

### 6a. `tests/Nuons.CodeInjection.Generators.Tests/DtoMappingGeneratorTests.cs`

```csharp
using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Generators.Tests;

public class DtoMappingGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
    : IClassFixture<NuonGeneratorFixture>
{
    [Fact]
    public Task AllPublicPropertiesAreMapped()
    {
        var sources = fixture.GenerateMappingSources("AllPublicPropertiesAreMapped.cs");
        return Verify(sources);
    }

    [Fact]
    public Task MapIgnoreExcludesProperties()
    {
        var sources = fixture.GenerateMappingSources("MapIgnoreExcludesProperties.cs");
        return Verify(sources);
    }

    [Fact]
    public Task NullablePropertiesArePreserved()
    {
        var sources = fixture.GenerateMappingSources("NullablePropertiesArePreserved.cs");
        return Verify(sources);
    }

    [Fact]
    public Task EnumPropertiesAreMapped()
    {
        var sources = fixture.GenerateMappingSources("EnumPropertiesAreMapped.cs");
        return Verify(sources);
    }

    [Fact]
    public Task UnsupportedTypesAreSkipped()
    {
        var sources = fixture.GenerateMappingSources("UnsupportedTypesAreSkipped.cs");
        return Verify(sources);
    }

    [Fact]
    public Task ReadOnlySourcePropertiesAreMapped()
    {
        var sources = fixture.GenerateMappingSources("ReadOnlySourcePropertiesAreMapped.cs");
        return Verify(sources);
    }

    [Fact]
    public Task ExtensionMethodIsGeneratedCorrectly()
    {
        var sources = fixture.GenerateMappingSources("ExtensionMethodIsGeneratedCorrectly.cs");
        return Verify(sources);
    }

    [Fact]
    public void NonPartialRecordIsSkipped()
    {
        var sources = fixture.GenerateMappingSources("NonPartialRecordIsSkipped.cs");
        Assert.Equal(string.Empty, sources);
    }
}
```

**Key details:**

- Each test case from spec Section 6 is a separate `[Fact]` with its own snapshot
- Test names match the spec's test case names exactly
- Each test uses its own sample file via `GenerateMappingSources("FileName.cs")`
- `NonPartialRecordIsSkipped` does NOT use Verify — it asserts empty output directly (no snapshot needed for empty output)
- All other tests use `Verify(sources)` which creates individual `.verified.txt` files per test

### 6b. Running and accepting snapshots

**First run** — all snapshot tests will fail because no `.verified.txt` files exist yet:

```bash
dotnet test tests/Nuons.CodeInjection.Generators.Tests/ --filter "DtoMappingGeneratorTests"
```

This creates `.received.txt` files for each test. **Review each received file carefully:**

| Test | What to check in the snapshot |
| ---- | ---- |
| `AllPublicPropertiesAreMapped` | All 6 `SampleOrder` properties present, `{ get; init; }`, `global::` prefixes |
| `MapIgnoreExcludesProperties` | `InternalTrackingId` is absent, other 5 properties present |
| `NullablePropertiesArePreserved` | `int?` -> `global::System.Nullable<global::System.Int32>`, `string?` -> `global::System.String?` |
| `EnumPropertiesAreMapped` | `Status` type is `global::Nuons.CodeInjection.Generators.Tests.SampleOrderStatus` |
| `UnsupportedTypesAreSkipped` | Only `Id`, `Name`, `Status`, `CreatedAt` — no `Tags` or `RelatedOrder` |
| `ReadOnlySourcePropertiesAreMapped` | `Id` and `Name` both present |
| `ExtensionMethodIsGeneratedCorrectly` | Static extension method, correct name `ToSampleOrderExtDto`, correct parameter type, correct body |
| `NonPartialRecordIsSkipped` | No snapshot — asserts empty string directly |

**Accept snapshots** by renaming each `.received.txt` to `.verified.txt`:

```bash
cd tests/Nuons.CodeInjection.Generators.Tests/
for f in DtoMappingGeneratorTests.*.received.txt; do mv "$f" "${f/received/verified}"; done
```

**Second run** — all should pass:

```bash
dotnet test tests/Nuons.CodeInjection.Generators.Tests/
```

**If any generated output is wrong**, do NOT accept that snapshot. Fix the generator or source builders, then re-run.

**Acceptance criteria:**

- [ ] Test class exists with 8 test methods (7 snapshot + 1 assertion)
- [ ] Each snapshot test produces its own `.verified.txt` file
- [ ] All 8 tests pass
- [ ] `NonPartialRecordIsSkipped` correctly asserts empty output
- [ ] All existing tests still pass (no regressions)

**Verification:**

```bash
dotnet test tests/Nuons.CodeInjection.Generators.Tests/
```

This runs ALL tests in the project, ensuring no regressions.

**STOP**: Ask the user "Step 6 is complete. Shall I continue with Step 7?"

---

## Step 7: Validate Against Acceptance Criteria

**Goal**: Systematically verify each acceptance criterion from the spec. This is a review/validation step, not a coding step. Fix any issues found.

**Checklist — verify each by inspecting the snapshot and/or running `DevRunGenerator`:**

| AC | Description | How to verify |
| ---- | ---- | ---- |
| AC-001 | All public gettable supported-type properties are generated | Check `SampleOrderDto` snapshot has all 6 properties |
| AC-002 | `MapIgnore` excludes properties | Check `SampleOrderWithIgnoreDto` snapshot lacks `InternalTrackingId` |
| AC-003 | Unsupported types silently skipped | Check `SampleComplexEntityDto` snapshot has only `Id`, `Name`, `Status`, `CreatedAt` |
| AC-004 | Extension method is correct (name, namespace, static) | Check all `*Extensions` classes in snapshot |
| AC-005 | Nullable properties preserved | Check `SampleProductDto` has `DiscountPercent` as nullable int and `Description` as nullable string |
| AC-006 | Read-only source properties included | Check `SampleReadOnlyEntityDto` has `Id` and `Name` |
| AC-007 | Enum properties use FQN | Check `Status` property type is `global::Nuons.CodeInjection.Generators.Tests.SampleOrderStatus` |
| AC-008 | Non-partial record skipped | Verify no output for non-partial types (test by absence) |
| AC-009 | All types use `global::` prefix | Scan snapshot for any type reference without `global::` |
| AC-010 | `.g.cs` naming convention | Check `Sources.GeneratedNameHint()` is used in generator |

**If any criterion fails**, fix the relevant code (generator, source builders, or samples) and re-run the tests. Update the snapshot if the fix changes the output.

**Acceptance criteria:**

- [ ] All 10 acceptance criteria from the spec are verified
- [ ] All tests pass
- [ ] No regressions in existing tests

**Verification:**

```bash
dotnet test tests/Nuons.CodeInjection.Generators.Tests/
```

**STOP**: Ask the user "Step 7 is complete. All acceptance criteria verified. Shall I continue with Step 8?"

---

## Step 8: Final Cleanup and Full Solution Build

**Goal**: Ensure the full solution builds cleanly, all tests pass across all projects, and no unnecessary files were introduced.

**Tasks:**

1. **Full solution build**:

   ```bash
   dotnet build
   ```

1. **Run all tests across the solution**:

   ```bash
   dotnet test
   ```

1. **Review for cleanup**:

   - No TODO comments left in new code
   - No unused `using` statements
   - No unnecessary files created
   - All new files follow existing naming conventions
   - No modifications to existing domain/source types (CON-005)

1. **Verify file inventory** — these are the only files that should be new or modified:

| File | Action |
| ---- | ---- |
| `src/Nuons.CodeInjection.Abstractions/MapFromAttribute.cs` | **New** |
| `src/Nuons.CodeInjection.Abstractions/MapIgnoreAttribute.cs` | **New** |
| `src/Nuons.CodeInjection.Generators/KnownCodeInjectionTypes.cs` | **Modified** (2 constants added) |
| `src/Nuons.CodeInjection.Generators/MappingIncrement.cs` | **New** |
| `src/Nuons.CodeInjection.Generators/MappedProperty.cs` | **New** |
| `src/Nuons.CodeInjection.Generators/MappingRecordSourceBuilder.cs` | **New** |
| `src/Nuons.CodeInjection.Generators/MappingExtensionSourceBuilder.cs` | **New** |
| `src/Nuons.CodeInjection.Generators/DtoMappingGenerator.cs` | **New** |
| `tests/Nuons.CodeInjection.Generators.Tests/MappingSamplesShared.cs` | **New** |
| `tests/Nuons.CodeInjection.Generators.Tests/MappingSamples/*.cs` (8 files) | **New** |
| `tests/Nuons.CodeInjection.Generators.Tests/FixtureExtensions.cs` | **Modified** |
| `tests/Nuons.CodeInjection.Generators.Tests/DtoMappingGeneratorTests.cs` | **New** |
| `tests/Nuons.CodeInjection.Generators.Tests/DtoMappingGeneratorTests.*.verified.txt` (7 files) | **New** |

**Acceptance criteria:**

- [ ] Full solution builds without errors or warnings
- [ ] All tests pass across all projects
- [ ] Only expected files were created/modified
- [ ] No regressions in any existing functionality

**Verification:**

```bash
dotnet build && dotnet test
```

**STOP**: Ask the user "Step 8 is complete. The DTO Mapping Source Generator implementation is finished. Would you like me to commit the changes?"

---

## Summary of All Steps

| Step | Description | Key Output |
| ---- | ---- | ---- |
| 1 | Add attributes | `MapFromAttribute.cs`, `MapIgnoreAttribute.cs` |
| 2 | Known types + data models | `KnownCodeInjectionTypes` updated, `MappingIncrement.cs`, `MappedProperty.cs` |
| 3 | Source builders | `MappingRecordSourceBuilder.cs`, `MappingExtensionSourceBuilder.cs` |
| 4 | Generator implementation | `DtoMappingGenerator.cs` |
| 5 | Test samples + fixture | `MappingSamplesShared.cs`, `MappingSamples/*.cs` (8 files), `FixtureExtensions.cs` updated |
| 6 | Tests + snapshots | `DtoMappingGeneratorTests.cs` (8 tests), 7 `.verified.txt` files |
| 7 | Acceptance criteria validation | Review + fixes |
| 8 | Final build + cleanup | Full solution green |
