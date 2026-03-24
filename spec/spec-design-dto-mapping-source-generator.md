---
title: DTO Mapping Source Generator
version: 1.0
date_created: 2026-03-24
last_updated: 2026-03-24
owner: Nikola Ljubisic
tags: [design, source-generator, code-injection, dto, mapping]
---

# Introduction

This specification defines a Roslyn incremental source generator that enables automatic DTO (Data Transfer Object) creation and mapping from domain objects. The generator is part of the Nuons Code Injection family and follows established patterns within the Nuons solution. Users define a partial record annotated with `[MapFrom(typeof(SourceType))]`, and the generator produces the matching properties and a `To{RecordName}()` extension method on the source type.

## 1. Purpose & Scope

**Purpose**: Provide a compile-time, zero-reflection mechanism for generating DTO records and their mapping methods from domain objects, eliminating boilerplate and reducing the risk of mapping drift when domain models evolve.

**Scope (v1)**:
- Generate properties on a user-defined partial record by cloning public properties from a source type
- Generate a `To{RecordName}()` extension method on the source type
- Support simple (non-nested) property types only: primitives, nullable primitives, enums, strings, `DateTime`, `DateTimeOffset`, `Guid`, `TimeSpan`, `decimal`
- Support property exclusion via `[MapIgnore("PropertyName")]`
- Records only (not classes)

**Out of scope (v1)**:
- Nested/complex object mapping
- Collection properties (`List<T>`, `IEnumerable<T>`, arrays)
- Reverse mapping (DTO to domain)
- Custom property name mapping or transformation
- Inheritance hierarchies
- Roslyn analyzers and diagnostics
- Class-based DTOs

**Intended audience**: .NET developers using the Nuons source generator library.

**Assumptions**:
- Users are familiar with C# partial types and records
- The domain (source) type has accessible public properties with getters
- The target Nuons solution structure and packaging model remain unchanged

## 2. Definitions

| Term | Definition |
|------|-----------|
| **DTO** | Data Transfer Object — a simple object used to transfer data between layers or systems, without business logic |
| **Domain object** | The source type containing business properties; must remain clean (no generator attributes) |
| **Source type** | The type whose properties are cloned into the DTO record (the domain object) |
| **Target type** | The user-defined partial record that receives the generated properties |
| **MapFrom** | The primary attribute that marks a partial record as a DTO generated from a source type |
| **MapIgnore** | A repeatable attribute on the target type to exclude specific source properties |
| **Property cloning** | Copying property definitions (name and type) from source to target at compile time |
| **Mapping method** | A generated extension method that creates a new DTO instance from a source object |
| **Incremental generator** | A Roslyn `IIncrementalGenerator` that participates in the compilation pipeline and produces source code |

## 3. Requirements, Constraints & Guidelines

### Requirements

- **REQ-001**: The generator SHALL produce a partial record extension containing all public gettable properties from the source type, minus any excluded by `[MapIgnore]`.
- **REQ-002**: The generator SHALL produce a static extension method `To{RecordName}()` on the source type that creates and returns a new instance of the target record, mapping each property by name.
- **REQ-003**: The `[MapFrom(typeof(T))]` attribute SHALL accept a single `Type` argument identifying the source type.
- **REQ-004**: The `[MapIgnore("PropertyName")]` attribute SHALL be repeatable (`AllowMultiple = true`) and accept a single `string` argument identifying the source property name to exclude.
- **REQ-005**: The generated extension method SHALL reside in the same namespace as the target record.
- **REQ-006**: The generated extension method name SHALL follow the pattern `To{RecordName}` where `{RecordName}` is the simple name of the target record type.
- **REQ-007**: The generator SHALL only process types that are declared as `partial record` (not `partial class` or non-partial types).
- **REQ-008**: Generated property types SHALL use fully qualified names with the `global::` prefix to avoid namespace conflicts, consistent with existing Nuons generators.
- **REQ-009**: Both attributes SHALL use `[Conditional(Constants.CodeGenerationCondition)]` so they are erased from compiled output, consistent with all other Nuons attributes.
- **REQ-010**: The generator SHALL support the following property types from the source: primitives (`bool`, `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`, `char`), `string`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`, `Guid`, enums, and their nullable variants.
- **REQ-011**: The generator SHALL skip (silently ignore) any source property whose type is not in the supported set defined by REQ-010.
- **REQ-012**: The generated record properties SHALL be declared as `init`-only (`{ get; init; }`) to preserve record immutability semantics.

### Constraints

- **CON-001**: Attributes MUST be placed in the `Nuons.CodeInjection.Abstractions` project and target `netstandard2.0`.
- **CON-002**: The generator MUST be placed in the `Nuons.CodeInjection.Generators` project and target `netstandard2.0`.
- **CON-003**: The generator MUST implement `IIncrementalGenerator` and use `ForAttributeWithMetadataName` for efficient attribute discovery.
- **CON-004**: The generator MUST be assembly-scoped only (no startup-scoped component required).
- **CON-005**: The domain (source) type MUST NOT require any Nuons attributes or modifications.
- **CON-006**: Generated source files MUST use the `.g.cs` naming convention via `Sources.GeneratedNameHint()`.
- **CON-007**: The generator MUST use the existing `Nuons.Core.Generators` infrastructure (`Sources`, `Syntax` helpers).

### Guidelines

- **GUD-001**: Follow the existing code structure and patterns established by `InjectConstructorGenerator` — extract an increment model, use a source builder, and register output via `SourceProductionContext`.
- **GUD-002**: Add FQN constants for the new attributes to `KnownCodeInjectionTypes` using the same pattern as existing entries.
- **GUD-003**: Use `StringBuilder`-based source building consistent with `InjectionSourceBuilder`.
- **GUD-004**: The generated code SHOULD be human-readable and properly indented using `Sources.Tab1` / `Sources.Tab2` constants.

### Patterns

- **PAT-001**: Attribute conditional erasure pattern — all attributes use `[Conditional(Constants.CodeGenerationCondition)]` to be stripped from compiled assemblies.
- **PAT-002**: Incremental generator pipeline pattern — `ForAttributeWithMetadataName` → extract increment → filter nulls → `RegisterSourceOutput`.
- **PAT-003**: Source builder pattern — dedicated builder class that accumulates configuration via `With()` calls and produces source text via `Build()`.
- **PAT-004**: Fully qualified type names — use `global::` prefix via `Syntax.ToFullTypeName()` for all type references in generated code.

## 4. Interfaces & Data Contracts

### 4.1 Attributes

#### `MapFromAttribute`

```csharp
// Project: Nuons.CodeInjection.Abstractions
// File: MapFromAttribute.cs
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

#### `MapIgnoreAttribute`

```csharp
// Project: Nuons.CodeInjection.Abstractions
// File: MapIgnoreAttribute.cs
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

### 4.2 Generator Increment Model

```csharp
// Internal model extracted during the pipeline's transform step
internal record MappingIncrement(
    string TargetNamespace,       // Namespace of the user's partial record
    string TargetRecordName,      // Simple name of the user's partial record
    string TargetAccessibility,   // "public" or "internal"
    string SourceFullTypeName,    // Fully qualified name of the source type (global::)
    string SourceSimpleName,      // Simple name of the source type
    IReadOnlyList<MappedProperty> Properties  // Properties to generate
);

internal record MappedProperty(
    string Name,         // Property name (same as source)
    string FullTypeName  // Fully qualified type name with global:: prefix
);
```

### 4.3 Generated Code Shape

Given user input:

```csharp
namespace MyApp.Contracts;

[MapFrom(typeof(MyApp.Domain.Order))]
[MapIgnore("InternalTrackingId")]
public partial record OrderDto;
```

Where the domain type is:

```csharp
namespace MyApp.Domain;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public OrderStatus Status { get; set; }
    public Guid InternalTrackingId { get; set; }
}
```

The generator produces **two source outputs**:

**File 1: `OrderDto.g.cs`** — Partial record with cloned properties

```csharp
namespace MyApp.Contracts;
public partial record OrderDto
{
    public global::System.Int32 Id { get; init; }
    public global::System.String CustomerName { get; init; }
    public global::MyApp.Domain.OrderStatus Status { get; init; }
}
```

**File 2: `OrderDtoExtensions.g.cs`** — Extension method for mapping

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
            Status = source.Status,
        };
    }
}
```

### 4.4 KnownCodeInjectionTypes Additions

```csharp
// Added to existing KnownCodeInjectionTypes.cs
public const string MapFromAttribute = "Nuons.CodeInjection.Abstractions.MapFromAttribute";
public const string MapIgnoreAttribute = "Nuons.CodeInjection.Abstractions.MapIgnoreAttribute";
```

## 5. Acceptance Criteria

- **AC-001**: Given a partial record annotated with `[MapFrom(typeof(SourceType))]`, when the generator runs, then the partial record is extended with `{ get; init; }` properties matching all public gettable properties of `SourceType` that have supported types.
- **AC-002**: Given a partial record annotated with `[MapFrom(typeof(SourceType))]` and one or more `[MapIgnore("PropName")]`, when the generator runs, then properties named in `MapIgnore` are excluded from the generated record and mapping method.
- **AC-003**: Given a source type with a property of an unsupported type (e.g., `List<string>`, a nested complex object), when the generator runs, then that property is silently skipped and the remaining supported properties are generated correctly.
- **AC-004**: Given a generated DTO, when inspecting the generated extension method, then it is a static extension method on the source type, named `To{RecordName}`, residing in the same namespace as the target record.
- **AC-005**: Given a source type with nullable properties (e.g., `int?`, `string?`), when the generator runs, then the nullability is preserved in the generated record properties.
- **AC-006**: Given a source type with `readonly` or `init`-only properties, when the generator runs, then those properties are included in the generated record (read from source, written via init on target).
- **AC-007**: Given a source type with enum properties, when the generator runs, then the enum properties are included with their fully qualified type names.
- **AC-008**: Given a type annotated with `[MapFrom]` that is NOT a partial record, when the generator runs, then no code is generated for that type (silently skipped in v1).
- **AC-009**: Given the generated code, when compiled, then all type references use `global::` prefixes and no namespace conflicts occur.
- **AC-010**: Given the generated code, when inspected, then the output files follow the `.g.cs` naming convention using `Sources.GeneratedNameHint()`.

## 6. Test Automation Strategy

### Test Levels

- **Unit tests**: Snapshot-based verification of generated source code using the existing `NuonGeneratorFixture` and `Verify.XunitV3` infrastructure.
- **End-to-end tests**: Compilation and runtime validation in `Nuons.EndToEnd.Api` or a feature module project.

### Test Structure

Tests SHALL be added to the existing `Nuons.CodeInjection.Generators.Tests` project, following the established patterns:

```
tests/Nuons.CodeInjection.Generators.Tests/
    MappingSamples.cs                    // Sample domain types and annotated records
    MapFromGeneratorTests.cs             // Snapshot verification tests
    MapFromGeneratorTests.*.verified.txt // Verified snapshot files
```

### Test Cases

| Test | Description |
|------|-------------|
| `AllPublicPropertiesAreMapped` | Source with multiple simple properties; all appear in generated record and mapping |
| `MapIgnoreExcludesProperties` | Source with `[MapIgnore]` on target; excluded properties do not appear |
| `NullablePropertiesArePreserved` | Source with `int?`, `string?` properties; nullability preserved |
| `EnumPropertiesAreMapped` | Source with enum properties; fully qualified enum type in output |
| `UnsupportedTypesAreSkipped` | Source with `List<T>` or nested object property; those are silently omitted |
| `ReadOnlySourcePropertiesAreMapped` | Source with `{ get; }` only properties; included in output |
| `ExtensionMethodIsGeneratedCorrectly` | Verify extension method name, namespace, and body |
| `NonPartialRecordIsSkipped` | Annotated non-partial class produces no output |

### Frameworks

- **Test runner**: xunit.v3 with Microsoft.Testing.Platform
- **Snapshot verification**: Verify.XunitV3
- **Test infrastructure**: `NuonGeneratorFixture`, `NuonGeneratorTestContext`

### Coverage Requirements

- All test cases listed above must pass
- Generated code must compile without errors or warnings when added to a test compilation

## 7. Rationale & Context

### Why a source generator?

The Nuons library is built entirely around compile-time code generation. A source generator approach for DTOs:
- Maintains zero runtime overhead (no reflection, no runtime mapping libraries)
- Catches mapping issues at compile time rather than runtime
- Stays consistent with the existing Nuons generator ecosystem

### Why records?

Records provide value equality, immutability via `init` setters, and concise syntax. For v1, restricting to records simplifies the generator (no need to handle mutable class patterns) and encourages best practices for DTO design. Class support can be added in a future version.

### Why user-defined partial records (not generated from domain)?

Placing the attribute on the DTO side (not the domain) keeps domain objects clean — they require zero Nuons-specific annotations. This is a deliberate architectural choice: the domain layer should not depend on or be aware of mapping infrastructure.

### Why silent skipping for unsupported types?

In v1, diagnostics and analyzers are deferred. Silently skipping unsupported properties is the simplest correct behavior — it avoids broken builds while the user can inspect the generated code to verify which properties were included.

### Why two generated files?

Separating the partial record (properties) from the extension method class (mapping) follows single-responsibility and makes the generated output easier to inspect. It also avoids potential conflicts if the user adds their own methods to the partial record.

## 8. Dependencies & External Integrations

### Technology Platform Dependencies

- **PLT-001**: .NET Standard 2.0 — Required target for both abstraction and generator projects, ensuring broad .NET compatibility
- **PLT-002**: Roslyn (Microsoft.CodeAnalysis.CSharp) — The incremental source generator API (`IIncrementalGenerator`, `ForAttributeWithMetadataName`)
- **PLT-003**: C# language features — `partial record` syntax requires C# 9.0+ consumers

### Internal Dependencies

- **INT-001**: `Nuons.Core.Abstractions` — Provides `Constants.CodeGenerationCondition` for conditional attribute erasure
- **INT-002**: `Nuons.Core.Generators` — Provides `Sources` (naming, formatting), `Syntax` (symbol helpers, type name formatting)
- **INT-003**: `Nuons.CodeInjection.Abstractions` — Target project for new attributes; provides the assembly marker type for test compilation references
- **INT-004**: `Nuons.CodeInjection.Generators` — Target project for the new generator; hosts existing `KnownCodeInjectionTypes`

### Test Dependencies

- **TST-001**: `Nuons.Core.Tests` — Provides `NuonGeneratorFixture` and `NuonGeneratorTestContext` for generator testing
- **TST-002**: Verify.XunitV3 — Snapshot testing framework for verifying generated source output

## 9. Examples & Edge Cases

### Example 1: Basic mapping

```csharp
// Domain (untouched)
namespace MyApp.Domain;
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

// User-defined DTO
namespace MyApp.Api.Contracts;

[MapFrom(typeof(MyApp.Domain.Product))]
public partial record ProductDto;

// Generated: ProductDto.g.cs
namespace MyApp.Api.Contracts;
public partial record ProductDto
{
    public global::System.Int32 Id { get; init; }
    public global::System.String Name { get; init; }
    public global::System.Decimal Price { get; init; }
    public global::System.Boolean IsActive { get; init; }
}

// Generated: ProductDtoExtensions.g.cs
namespace MyApp.Api.Contracts;
public static class ProductDtoExtensions
{
    public static ProductDto ToProductDto(this global::MyApp.Domain.Product source)
    {
        return new ProductDto
        {
            Id = source.Id,
            Name = source.Name,
            Price = source.Price,
            IsActive = source.IsActive,
        };
    }
}
```

### Example 2: With property exclusion

```csharp
namespace MyApp.Api.Contracts;

[MapFrom(typeof(MyApp.Domain.User))]
[MapIgnore("PasswordHash")]
[MapIgnore("SecurityStamp")]
public partial record UserDto;
```

Only properties NOT named `PasswordHash` or `SecurityStamp` are generated.

### Example 3: Mixed supported and unsupported properties

```csharp
// Domain
public class Order
{
    public int Id { get; set; }                    // supported -> included
    public string CustomerName { get; set; }       // supported -> included
    public List<OrderItem> Items { get; set; }     // unsupported -> skipped
    public Address ShippingAddress { get; set; }   // unsupported -> skipped
    public OrderStatus Status { get; set; }        // enum -> included
    public DateTime? ShippedAt { get; set; }       // nullable DateTime -> included
}
```

Generated record contains only: `Id`, `CustomerName`, `Status`, `ShippedAt`.

### Edge Case 1: Source type with no supported properties

If all properties on the source type are unsupported (or all are excluded via `[MapIgnore]`), the generator produces an empty partial record and an extension method that returns `new RecordName {}`.

### Edge Case 2: Property name collision with record members

Records have synthesized members (e.g., `EqualityContract`). The generator should not produce properties that conflict with these. In practice, domain objects are unlikely to have such names, but if they do, the property should be skipped.

### Edge Case 3: Internal source type

If the source type is `internal`, the extension method must also account for accessibility. The extension method class should match the lower accessibility of the source type and the target record.

### Edge Case 4: Source type in a different assembly

The source type referenced in `[MapFrom(typeof(T))]` may reside in a different assembly. The generator resolves the type via the Roslyn semantic model, which handles cross-assembly references naturally as long as the project has the appropriate dependency.

## 10. Validation Criteria

- All acceptance criteria (AC-001 through AC-010) pass
- All test cases defined in Section 6 pass with snapshot verification
- Generated code compiles without errors when included in a test compilation
- Generated code follows Nuons formatting conventions (tabs, `global::` prefixes, `.g.cs` naming)
- Attributes are erased from compiled output (conditional compilation verified)
- No modifications required to any existing domain/source types
- The generator integrates cleanly into the existing `Nuons.CodeInjection.Generators` project without breaking existing `InjectConstructorGenerator` functionality

## 11. Related Specifications / Further Reading

- [Nuons Solution Architecture](/documentation/nuons-solution-architecture.md)
- [Nuons Code Injection Documentation](/documentation/nuons-code-injection.md)
- [Roslyn Incremental Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)
- [C# Records Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
