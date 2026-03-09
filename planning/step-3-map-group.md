# Step 3: Generate MapGroup() Instead of Route String Concatenation

## Goal

Change the generated code to use ASP.NET Core's `MapGroup()` per handler class instead of concatenating route prefixes into flat strings. Each handler's class-level `[Route]` prefix becomes a `MapGroup()` call, and individual endpoints use sub-routes relative to the group.

This is a prerequisite for Step 4 (endpoint configuration attributes), which needs `MapGroup()` to apply class-level configuration to all endpoints in a handler.

## Current Behavior

```csharp
// Currently generated:
app.MapGet("/domain-object/{id}", (DomainObjectHandler handler, Guid id) => handler.Get(id));
app.MapPost("/domain-object", (DomainObjectHandler handler, DomainObject obj) => handler.Create(obj));
```

## Target Behavior

```csharp
// After this step:
var domainObjectHandlerGroup = app.MapGroup("/domain-object");
domainObjectHandlerGroup.MapGet("{id}", (DomainObjectHandler handler, Guid id) => handler.Get(id));
domainObjectHandlerGroup.MapPost("", (DomainObjectHandler handler, DomainObject obj) => handler.Create(obj));
```

## Files to Modify

| File | Action |
|------|--------|
| `src/Nuons.Http.Generators/EndpointIncrement.cs` | **Modify** - add route prefix to `EndpointIncrement`, change `EndpointRegistration.Route` to sub-route only |
| `src/Nuons.Http.Generators/EndpointGenerator.cs` | **Modify** - store route prefix on increment, store sub-route on registration |
| `src/Nuons.Http.Generators/EndpointSourceBuilder.cs` | **Modify** - generate MapGroup + group.MapX pattern |
| `tests/Nuons.Http.Generators.Tests/EndpointGeneratorTests.EndpointExtensionsAreGeneratedCorrectly.verified.txt` | **Update** |

## Implementation Details

### 1. Update `EndpointIncrement` Record in `EndpointIncrement.cs`

Add the route prefix (from the class-level `[Route]` attribute) to `EndpointIncrement`:

```csharp
// Change from:
internal record EndpointIncrement(string HandlerType, ImmutableArray<EndpointRegistration> Endpoints);

// To:
internal record EndpointIncrement(string HandlerType, string RoutePrefix, ImmutableArray<EndpointRegistration> Endpoints);
```

The `Route` field on `EndpointRegistration` will now hold only the sub-route (method-level), not the full concatenated route.

### 2. Update `EndpointGenerator.cs`

In `ExtractIncrement()` method (lines 72-122):
- The route prefix is already extracted from the `[Route]` attribute on the class. Currently it's passed to `RouteBuilder` which concatenates it with sub-routes.
- Instead, store the route prefix directly on the `EndpointIncrement`.
- In the second overload of `TryCreateEndpoint`, use only the sub-route from the method attribute (not the concatenated result from `RouteBuilder`).

Changes:
1. Pass the route prefix string to `EndpointIncrement` constructor
2. In `TryCreateEndpoint`, extract the sub-route from the method attribute directly and store it on `EndpointRegistration` without concatenation
3. `RouteBuilder` is no longer used for concatenation — it may still be useful for normalizing the prefix (trimming slashes, ensuring leading slash). Evaluate whether to keep it for prefix normalization or inline that logic.

### 3. Update `EndpointSourceBuilder.cs`

This is the main change. Currently the builder iterates endpoints and emits `app.MapX(fullRoute, ...)`. Change to:

```csharp
// For each increment (handler class):
// 1. Emit: var {groupVarName} = app.MapGroup("{routePrefix}");
// 2. For each endpoint:
//    Emit: {groupVarName}.Map{HttpMethod}("{subRoute}", (params) => handler.Method(args));

foreach (var increment in increments)
{
    var groupVarName = GenerateGroupVariableName(increment.HandlerType);
    sb.AppendLine($"        var {groupVarName} = app.MapGroup(\"{increment.RoutePrefix}\");");

    foreach (var endpoint in increment.Endpoints)
    {
        // Build lambda parameters and call as before
        var lambdaParams = BuildLambdaParams(increment, endpoint);
        var callArgs = BuildCallArgs(endpoint);
        sb.AppendLine($"        {groupVarName}.Map{endpoint.HttpMethod}(\"{endpoint.Route}\", ({lambdaParams}) => handler.{endpoint.HandlerMethod}({callArgs}));");
    }
}
```

Generate a safe variable name from the handler type name (e.g., `DomainObjectHandler` -> `domainObjectHandlerGroup`). Use camelCase + "Group" suffix. Handle potential naming conflicts if needed.

### 4. Handle Edge Cases

- **Empty route prefix** (`[Route]` or `[Route("")]`): Emit `app.MapGroup("/")` or `app.MapGroup("")` — both are valid in ASP.NET Core.
- **Empty sub-route** (`[Get]` or `[Get("")]`): Emit `group.MapGet("", ...)` which maps to the group root.
- **Route prefix normalization**: Ensure the prefix has a leading `/` and no trailing `/`.
- **Handlers from referenced assemblies**: Same treatment — `SubDomainObjectHandler` from `SamplesReferences.cs` should also get its own group.

## Test Plan

### Update Snapshot

The snapshot file will change significantly. Every handler will now use the group pattern. Expected output shape:

```csharp
var emptyHandlerGroup = app.MapGroup("/empty");

var complexHandlerGroup = app.MapGroup("/complex");
complexHandlerGroup.MapGet("{id}", (ComplexHandler handler, string id) => handler.Get(id));
complexHandlerGroup.MapPost("", (ComplexHandler handler, ComplexModel model) => handler.Create(model));

var domainObjectHandlerGroup = app.MapGroup("/domain-object");
domainObjectHandlerGroup.MapGet("{id}", (DomainObjectHandler handler, Guid id) => handler.Get(id));
// ... etc

var subDomainObjectHandlerGroup = app.MapGroup("/sub-domain-object");
subDomainObjectHandlerGroup.MapGet("{id}", (SubDomainObjectHandler handler, Guid id) => handler.Get(id));
```

### Existing RouteBuilder Tests

The tests in `tests/Nuons.Http.Generators.Tests/RouteBuilderTests.cs` may need updating depending on whether `RouteBuilder` is kept, modified, or removed. If `RouteBuilder` is still used for prefix normalization, its tests stay. If removed, delete the test file.

### Verify

1. All handlers get their own `MapGroup()` call
2. Sub-routes are relative to the group (no prefix duplication)
3. Empty handlers (no endpoints) still get a group (or are skipped — decide based on whether an empty group is useful)
4. Cross-assembly handlers (`SubDomainObjectHandler`) work correctly
5. Route normalization works: no double slashes, proper leading slash on prefix
6. The `AddNuonHttpServices()` method is unchanged (service registration is unaffected)

## Documentation Update

Update the "Route Building" section in `documentation/nuons-http.md` to reflect the new `MapGroup()` approach:

Replace the existing Route Building section with:

```markdown
### Route Building

Each handler class generates a `MapGroup()` call using the base route from the `Route` attribute.
Individual endpoints within the handler are registered on the group using their sub-route.

For example, a handler with `[Route("orders")]` and a method with `[Get("{id}")]` generates:
```csharp
var ordersHandlerGroup = app.MapGroup("/orders");
ordersHandlerGroup.MapGet("{id}", ...);
```

This grouping enables shared configuration (authorization, CORS, rate limiting) to be applied once at the group level.
```

## Build & Test Commands

```bash
dotnet build src/Nuons.Http.Generators/Nuons.Http.Generators.csproj
dotnet test tests/Nuons.Http.Generators.Tests/Nuons.Http.Generators.Tests.csproj
```
