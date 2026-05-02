# Step 2: Add Patch Attribute

## Goal

Add PATCH HTTP method support. `HttpMethods.cs` already defines a `Patch` constant but there is no `PatchAttribute`, no entry in `KnownHttpTypes`, `EndpointAttributes`, or `TryCreateEndpoint`. This step closes that gap and also refactors the `TryCreateEndpoint` dispatch from repeated `if` statements to a loop.

## Files to Modify

| File | Action |
|------|--------|
| `src/Nuons.Http.Abstractions/PatchAttribute.cs` | **Create** |
| `src/Nuons.Http.Generators/KnownHttpTypes.cs` | **Modify** - add Patch constant |
| `src/Nuons.Http.Generators/EndpointAttributes.cs` | **Modify** - add Patch field + update factory |
| `src/Nuons.Http.Generators/EndpointGenerator.cs` | **Modify** - refactor TryCreateEndpoint |
| `tests/Nuons.Http.Generators.Tests/Samples.cs` | **Modify** - add PATCH sample |
| `tests/Nuons.Http.Generators.Tests/EndpointGeneratorTests.EndpointExtensionsAreGeneratedCorrectly.verified.txt` | **Update** |

## Implementation Details

### 1. Create `PatchAttribute.cs` in `src/Nuons.Http.Abstractions/`

Follow the exact same pattern as `GetAttribute.cs`:

```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class PatchAttribute : Attribute
{
    public PatchAttribute(string route = "") { }
}
```

### 2. Add Constant to `KnownHttpTypes.cs`

Add after the existing `DeleteAttribute` constant:

```csharp
public const string PatchAttribute = "Nuons.Http.Abstractions.PatchAttribute";
```

### 3. Update `EndpointAttributes` Record in `EndpointAttributes.cs`

Add `Patch` to the record:

```csharp
internal record EndpointAttributes(
    INamedTypeSymbol Route,
    INamedTypeSymbol Get,
    INamedTypeSymbol Post,
    INamedTypeSymbol Put,
    INamedTypeSymbol Delete,
    INamedTypeSymbol Patch
)
```

Update the `FromCompilation` factory method to resolve the Patch attribute symbol:

```csharp
var patch = compilation.GetTypeByMetadataName(KnownHttpTypes.PatchAttribute);
```

Add it to the null check and the return statement.

Also add a helper to iterate over HTTP method attributes. Add this method to `EndpointAttributes`:

```csharp
public IEnumerable<(INamedTypeSymbol Symbol, string HttpMethod)> HttpMethodAttributes()
{
    yield return (Get, HttpMethods.Get);
    yield return (Post, HttpMethods.Post);
    yield return (Put, HttpMethods.Put);
    yield return (Delete, HttpMethods.Delete);
    yield return (Patch, HttpMethods.Patch);
}
```

### 4. Refactor `TryCreateEndpoint` in `EndpointGenerator.cs`

Replace the first overload of `TryCreateEndpoint` (lines 125-165) which currently has 4 separate `if` blocks (and a TODO comment about this). Replace with a loop:

```csharp
// Current code has:
// // TODO foreach on endpoint spec instead of 4 IFs
// if (method has Get) TryCreateEndpoint(..., HttpMethods.Get, ...)
// if (method has Post) TryCreateEndpoint(..., HttpMethods.Post, ...)
// if (method has Put) TryCreateEndpoint(..., HttpMethods.Put, ...)
// if (method has Delete) TryCreateEndpoint(..., HttpMethods.Delete, ...)

// Replace with:
foreach (var (attributeSymbol, httpMethod) in attributes.HttpMethodAttributes())
{
    if (method.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol)))
    {
        TryCreateEndpoint(method, routeBuilder, httpMethod, attributeSymbol, endpointBuilder);
    }
}
```

Note: Check how the existing code matches attributes — it may use `SymbolEqualityComparer.Default.Equals` or check the attribute class directly. Follow the same pattern.

### 5. Verify `HttpMethods.cs` Already Has Patch

The file at `src/Nuons.Http.Generators/HttpMethods.cs` already contains:

```csharp
public const string Patch = "Patch";
```

No change needed here. The `MapPatch` method will be emitted by `EndpointSourceBuilder` which uses `$"app.Map{httpMethod}(..."` — so `Map` + `Patch` = `MapPatch` which is the correct ASP.NET Core method.

## Test Plan

### Add Test Sample

Add a PATCH method to an existing handler or create a new one in `tests/Nuons.Http.Generators.Tests/Samples.cs`:

```csharp
// Add to an existing handler class, e.g., DomainObjectHandler:
[Patch("{id}")]
public void Patch(Guid id, DomainObject obj) { }
```

Or create a dedicated handler:

```csharp
[Route("patchable")]
public class PatchableHandler
{
    [Get("{id}")]
    public string Get(Guid id) => "";

    [Patch("{id}")]
    public void Patch(Guid id, PatchRequest request) { }
}
```

### Update Snapshot

Run the test and accept the new snapshot output. The snapshot should now contain a `MapPatch` call:

```csharp
app.MapPatch("/patchable/{id}", (PatchableHandler handler, Guid id, PatchRequest request) => handler.Patch(id, request));
```

### Verify

1. All existing tests pass — the refactored loop produces identical output for GET/POST/PUT/DELETE
2. New PATCH endpoint appears in generated code
3. A method with multiple HTTP attributes (e.g., both `[Get]` and `[Post]`) still generates multiple endpoints (verify the loop handles this correctly, same as the old if-chain)

## Documentation Update

Update `documentation/nuons-http.md`:

1. Add `Patch` to the HTTP method attributes list (after the `Delete` bullet):
   ```markdown
   - `Patch`: Maps an HTTP PATCH request.
   ```

2. Update the generation logic description (line 25) to include Patch:
   ```markdown
   The source generator scans for classes marked with `Route` and methods marked with `Get`, `Post`, `Put`, `Delete`, or `Patch`.
   ```

## Build & Test Commands

```bash
dotnet build src/Nuons.Http.Abstractions/Nuons.Http.Abstractions.csproj
dotnet build src/Nuons.Http.Generators/Nuons.Http.Generators.csproj
dotnet test tests/Nuons.Http.Generators.Tests/Nuons.Http.Generators.Tests.csproj
```
