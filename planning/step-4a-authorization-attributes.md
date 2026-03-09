# Step 4A: Authorization Attributes

## Goal

Add `[Authorize]` and `[AllowAnonymous]` attributes to `Nuons.Http.Abstractions` and update the generator to emit `.RequireAuthorization()` and `.AllowAnonymous()` configuration calls on the appropriate group or endpoint.

**Prerequisite:** Step 3 (MapGroup) must be completed first.

## Design Context

- `[Authorize]` can be applied to classes (applies to the group) or methods (applies to individual endpoints)
- `[AllowAnonymous]` applies to methods only (overrides class-level `[Authorize]`)
- The generator translates these to fluent API calls, not ASP.NET attributes

## Translation Mapping

| Nuons Attribute | Target | Generated Code |
|----------------|--------|----------------|
| `[Authorize]` on class | Group | `group.RequireAuthorization()` |
| `[Authorize("Policy")]` on class | Group | `group.RequireAuthorization("Policy")` |
| `[Authorize]` on method | Endpoint | `.RequireAuthorization()` |
| `[Authorize("Policy")]` on method | Endpoint | `.RequireAuthorization("Policy")` |
| `[AllowAnonymous]` on method | Endpoint | `.AllowAnonymous()` |

## Files to Modify

| File | Action |
|------|--------|
| `src/Nuons.Http.Abstractions/AuthorizeAttribute.cs` | **Create** |
| `src/Nuons.Http.Abstractions/AllowAnonymousAttribute.cs` | **Create** |
| `src/Nuons.Http.Generators/KnownHttpTypes.cs` | **Modify** - add constants |
| `src/Nuons.Http.Generators/EndpointIncrement.cs` | **Modify** - add configuration data |
| `src/Nuons.Http.Generators/EndpointGenerator.cs` | **Modify** - extract config attributes |
| `src/Nuons.Http.Generators/EndpointSourceBuilder.cs` | **Modify** - emit config calls |
| `tests/Nuons.Http.Generators.Tests/Samples.cs` | **Modify** - add test handlers |
| `tests/Nuons.Http.Generators.Tests/EndpointGeneratorTests.EndpointExtensionsAreGeneratedCorrectly.verified.txt` | **Update** |

## Implementation Details

### 1. Create Attribute Classes

**AuthorizeAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class AuthorizeAttribute : Attribute
{
    public AuthorizeAttribute(string policy = "") { }
}
```

**AllowAnonymousAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class AllowAnonymousAttribute : Attribute
{
    public AllowAnonymousAttribute() { }
}
```

### 2. Add Constants to `KnownHttpTypes.cs`

```csharp
public const string AuthorizeAttribute = "Nuons.Http.Abstractions.AuthorizeAttribute";
public const string AllowAnonymousAttribute = "Nuons.Http.Abstractions.AllowAnonymousAttribute";
```

### 3. Add Configuration Data to Records in `EndpointIncrement.cs`

Add a model for endpoint configuration calls:

```csharp
internal record EndpointConfigurationCall(string Method, string? Argument);
```

Add configuration to both `EndpointIncrement` (class-level) and `EndpointRegistration` (method-level):

```csharp
internal record EndpointRegistration(
    string HttpMethod,
    string Route,
    string HandlerMethod,
    ImmutableArray<MethodParameter> HandlerMethodParameters,
    ImmutableArray<EndpointConfigurationCall> ConfigurationCalls);

internal record EndpointIncrement(
    string HandlerType,
    string RoutePrefix,
    ImmutableArray<EndpointRegistration> Endpoints,
    ImmutableArray<EndpointConfigurationCall> GroupConfigurationCalls);
```

### 4. Extract Configuration in `EndpointGenerator.cs`

In `ExtractIncrement()`, after processing the class symbol, check for class-level attributes:

```csharp
var groupConfigBuilder = ImmutableArray.CreateBuilder<EndpointConfigurationCall>();

foreach (var attr in classSymbol.GetAttributes())
{
    var fullName = attr.AttributeClass?.ToDisplayString();
    if (fullName == KnownHttpTypes.AuthorizeAttribute)
    {
        var policy = GetStringArg(attr);
        groupConfigBuilder.Add(new EndpointConfigurationCall("RequireAuthorization", policy));
    }
}
```

In `TryCreateEndpoint()`, check for method-level attributes:

```csharp
var configBuilder = ImmutableArray.CreateBuilder<EndpointConfigurationCall>();

foreach (var attr in method.GetAttributes())
{
    var fullName = attr.AttributeClass?.ToDisplayString();
    if (fullName == KnownHttpTypes.AuthorizeAttribute)
    {
        var policy = GetStringArg(attr);
        configBuilder.Add(new EndpointConfigurationCall("RequireAuthorization", policy));
    }
    else if (fullName == KnownHttpTypes.AllowAnonymousAttribute)
    {
        configBuilder.Add(new EndpointConfigurationCall("AllowAnonymous", null));
    }
}
```

Add helper:
```csharp
private static string? GetStringArg(AttributeData attr)
{
    if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string s && !string.IsNullOrEmpty(s))
        return s;
    return null;
}
```

### 5. Emit Configuration Calls in `EndpointSourceBuilder.cs`

For group-level configuration:
```csharp
var groupVarName = ...;
sb.AppendLine($"        var {groupVarName} = app.MapGroup(\"{increment.RoutePrefix}\");");

foreach (var config in increment.GroupConfigurationCalls)
{
    if (config.Argument is not null)
        sb.AppendLine($"        {groupVarName}.{config.Method}(\"{config.Argument}\");");
    else
        sb.AppendLine($"        {groupVarName}.{config.Method}();");
}
```

For endpoint-level configuration, chain calls on the `MapX` line:
```csharp
// If endpoint has config calls, don't end with semicolon yet
sb.Append($"        {groupVarName}.Map{endpoint.HttpMethod}(\"{endpoint.Route}\", ({lambdaParams}) => handler.{endpoint.HandlerMethod}({callArgs}))");

foreach (var config in endpoint.ConfigurationCalls)
{
    if (config.Argument is not null)
        sb.Append($".{config.Method}(\"{config.Argument}\")");
    else
        sb.Append($".{config.Method}()");
}
sb.AppendLine(";");
```

### Expected Generated Output

```csharp
// Class-level [Authorize("Admin")]
var securedHandlerGroup = app.MapGroup("/secured");
securedHandlerGroup.RequireAuthorization("Admin");
securedHandlerGroup.MapGet("{id}", (SecuredHandler handler, Guid id) => handler.Get(id));
securedHandlerGroup.MapPost("", (SecuredHandler handler, CreateRequest request) => handler.Create(request)).AllowAnonymous();

// Method-level [Authorize]
var mixedHandlerGroup = app.MapGroup("/mixed");
mixedHandlerGroup.MapGet("{id}", (MixedHandler handler, Guid id) => handler.Get(id)).RequireAuthorization();
mixedHandlerGroup.MapPost("", (MixedHandler handler, CreateRequest request) => handler.Create(request));
```

## Test Plan

### Add Test Samples to `Samples.cs`

```csharp
[Authorize("AdminPolicy")]
[Route("secured")]
public class SecuredHandler
{
    [Get("{id}")]
    public string Get(Guid id) => "";

    [AllowAnonymous]
    [Post]
    public void Create(string body) { }
}

[Route("mixed-auth")]
public class MixedAuthHandler
{
    [Authorize]
    [Get("{id}")]
    public string Get(Guid id) => "";

    [Authorize("EditorPolicy")]
    [Put("{id}")]
    public void Update(Guid id, string body) { }

    [Get]
    public string List() => "";
}
```

### Update Snapshot

The snapshot should show:
1. `SecuredHandler` group with `RequireAuthorization("AdminPolicy")` on the group and `.AllowAnonymous()` on the POST endpoint
2. `MixedAuthHandler` group with no group-level auth, `.RequireAuthorization()` on GET, `.RequireAuthorization("EditorPolicy")` on PUT, and no config on the List GET

### Verify

1. Class-level `[Authorize]` emits `group.RequireAuthorization()`
2. Class-level `[Authorize("Policy")]` emits `group.RequireAuthorization("Policy")`
3. Method-level `[Authorize]` emits `.RequireAuthorization()` chained on the endpoint
4. Method-level `[Authorize("Policy")]` emits `.RequireAuthorization("Policy")` chained on the endpoint
5. `[AllowAnonymous]` emits `.AllowAnonymous()` chained on the endpoint
6. Handlers without any auth attributes generate unchanged code (no config calls)
7. Unknown attributes are silently ignored

## Documentation Update

Add a new section to `documentation/nuons-http.md` after the Parameter Binding Attributes section:

```markdown
## Endpoint Configuration Attributes

### Authorization

- `Authorize(policy)`: Applied to a class or method. On a class, generates `group.RequireAuthorization()`. On a method, chains `.RequireAuthorization()` on the endpoint. The optional `policy` parameter specifies the authorization policy name.
- `AllowAnonymous`: Applied to a method. Chains `.AllowAnonymous()` on the endpoint, overriding class-level authorization.

These attributes are Nuons-specific (not ASP.NET Core attributes) and are translated to the corresponding minimal API configuration calls by the generator.
```

## Build & Test Commands

```bash
dotnet build src/Nuons.Http.Abstractions/Nuons.Http.Abstractions.csproj
dotnet build src/Nuons.Http.Generators/Nuons.Http.Generators.csproj
dotnet test tests/Nuons.Http.Generators.Tests/Nuons.Http.Generators.Tests.csproj
```
