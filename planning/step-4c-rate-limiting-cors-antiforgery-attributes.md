# Step 4C: Rate Limiting, CORS, and Antiforgery Attributes

## Goal

Add `[EnableRateLimiting]`, `[RequireCors]`, and `[DisableAntiforgery]` attributes to `Nuons.Http.Abstractions` and update the generator to emit `.RequireRateLimiting()`, `.RequireCors()`, and `.DisableAntiforgery()` configuration calls.

**Prerequisite:** Step 4B (OpenAPI Metadata) must be completed first, as it refines the configuration call infrastructure.

## Translation Mapping

| Nuons Attribute | Target | Generated Code |
|----------------|--------|----------------|
| `[EnableRateLimiting("policy")]` on class | Group | `group.RequireRateLimiting("policy")` |
| `[EnableRateLimiting("policy")]` on method | Endpoint | `.RequireRateLimiting("policy")` |
| `[RequireCors("policy")]` on class | Group | `group.RequireCors("policy")` |
| `[RequireCors("policy")]` on method | Endpoint | `.RequireCors("policy")` |
| `[DisableAntiforgery]` on class | Group | `group.DisableAntiforgery()` |
| `[DisableAntiforgery]` on method | Endpoint | `.DisableAntiforgery()` |

## Files to Modify

| File | Action |
|------|--------|
| `src/Nuons.Http.Abstractions/EnableRateLimitingAttribute.cs` | **Create** |
| `src/Nuons.Http.Abstractions/RequireCorsAttribute.cs` | **Create** |
| `src/Nuons.Http.Abstractions/DisableAntiforgeryAttribute.cs` | **Create** |
| `src/Nuons.Http.Generators/KnownHttpTypes.cs` | **Modify** - add constants |
| `src/Nuons.Http.Generators/EndpointGenerator.cs` | **Modify** - extract new attributes |
| `tests/Nuons.Http.Generators.Tests/Samples.cs` | **Modify** - add test handlers |
| `tests/Nuons.Http.Generators.Tests/EndpointGeneratorTests.EndpointExtensionsAreGeneratedCorrectly.verified.txt` | **Update** |

## Implementation Details

### 1. Create Attribute Classes

**EnableRateLimitingAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EnableRateLimitingAttribute : Attribute
{
    public EnableRateLimitingAttribute(string policy) { }
}
```

**RequireCorsAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class RequireCorsAttribute : Attribute
{
    public RequireCorsAttribute(string policy) { }
}
```

**DisableAntiforgeryAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class DisableAntiforgeryAttribute : Attribute
{
    public DisableAntiforgeryAttribute() { }
}
```

### 2. Add Constants to `KnownHttpTypes.cs`

```csharp
public const string EnableRateLimitingAttribute = "Nuons.Http.Abstractions.EnableRateLimitingAttribute";
public const string RequireCorsAttribute = "Nuons.Http.Abstractions.RequireCorsAttribute";
public const string DisableAntiforgeryAttribute = "Nuons.Http.Abstractions.DisableAntiforgeryAttribute";
```

### 3. Extract Attributes in `EndpointGenerator.cs`

Add to both the class-level and method-level attribute extraction loops (same pattern as 4A and 4B):

```csharp
// EnableRateLimiting
if (fullName == KnownHttpTypes.EnableRateLimitingAttribute)
{
    var policy = attr.ConstructorArguments[0].Value as string;
    if (policy is not null)
        configBuilder.Add(new EndpointConfigurationCall("RequireRateLimiting", ImmutableArray.Create(policy)));
}

// RequireCors
if (fullName == KnownHttpTypes.RequireCorsAttribute)
{
    var policy = attr.ConstructorArguments[0].Value as string;
    if (policy is not null)
        configBuilder.Add(new EndpointConfigurationCall("RequireCors", ImmutableArray.Create(policy)));
}

// DisableAntiforgery
if (fullName == KnownHttpTypes.DisableAntiforgeryAttribute)
{
    configBuilder.Add(new EndpointConfigurationCall("DisableAntiforgery", ImmutableArray<string>.Empty));
}
```

No changes needed to `EndpointSourceBuilder.cs` — the generic configuration call emission from Steps 4A/4B already handles these patterns.

### Expected Generated Output

```csharp
var rateLimitedGroup = app.MapGroup("/rate-limited");
rateLimitedGroup.RequireRateLimiting("fixed");
rateLimitedGroup.RequireCors("AllowAll");
rateLimitedGroup.MapGet("{id}", (RateLimitedHandler handler, Guid id) => handler.Get(id));
rateLimitedGroup.MapPost("", (RateLimitedHandler handler, string body) => handler.Create(body)).DisableAntiforgery();
```

## Test Plan

### Add Test Samples to `Samples.cs`

```csharp
[EnableRateLimiting("fixed")]
[RequireCors("AllowAll")]
[Route("rate-limited")]
public class RateLimitedHandler
{
    [Get("{id}")]
    public string Get(Guid id) => "";

    [DisableAntiforgery]
    [Post]
    public void Create(string body) { }
}

[Route("cors-only")]
public class CorsHandler
{
    [RequireCors("SpecificOrigin")]
    [Get]
    public string List() => "";

    [EnableRateLimiting("sliding")]
    [DisableAntiforgery]
    [Post]
    public void Create(string body) { }
}
```

### Update Snapshot

Verify the snapshot includes:
1. `RateLimitedHandler` group with `RequireRateLimiting("fixed")` and `RequireCors("AllowAll")` on the group
2. POST endpoint with `.DisableAntiforgery()`
3. `CorsHandler` with per-endpoint configuration: `.RequireCors("SpecificOrigin")` on GET, `.RequireRateLimiting("sliding").DisableAntiforgery()` chained on POST

### Verify

1. `[EnableRateLimiting]` emits `.RequireRateLimiting("policy")` at class or method level
2. `[RequireCors]` emits `.RequireCors("policy")` at class or method level
3. `[DisableAntiforgery]` emits `.DisableAntiforgery()` at class or method level
4. Multiple configuration calls chain correctly (e.g., `.RequireRateLimiting("sliding").DisableAntiforgery()`)
5. All previously added attributes (auth from 4A, OpenAPI from 4B) still work
6. Handlers without these attributes generate unchanged code
7. Combined scenario: a handler with `[Authorize]`, `[Tags]`, `[EnableRateLimiting]`, and `[RequireCors]` all at once generates all config calls

## Documentation Update

Add to the "Endpoint Configuration Attributes" section in `documentation/nuons-http.md` (after the OpenAPI Metadata subsection):

```markdown
### Rate Limiting, CORS, and Antiforgery

- `EnableRateLimiting(policy)`: Applied to a class or method. Generates `.RequireRateLimiting("policy")`.
- `RequireCors(policy)`: Applied to a class or method. Generates `.RequireCors("policy")`.
- `DisableAntiforgery`: Applied to a class or method. Generates `.DisableAntiforgery()`.

All endpoint configuration attributes can be combined. Class-level attributes apply to the `MapGroup()` and affect all endpoints in the handler. Method-level attributes chain on the individual endpoint registration.
```

## Build & Test Commands

```bash
dotnet build src/Nuons.Http.Abstractions/Nuons.Http.Abstractions.csproj
dotnet build src/Nuons.Http.Generators/Nuons.Http.Generators.csproj
dotnet test tests/Nuons.Http.Generators.Tests/Nuons.Http.Generators.Tests.csproj
```
