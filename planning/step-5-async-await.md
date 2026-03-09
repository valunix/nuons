# Step 5: Add Await for Async Handler Methods

## Goal

Detect handler methods that return `Task`, `Task<T>`, `ValueTask`, or `ValueTask<T>` and generate `async` lambdas with `await` in the generated code.

## Current Behavior

```csharp
// Currently generated (even for async methods):
group.MapGet("{id}", (Handler handler, Guid id) => handler.GetAsync(id));
```

## Target Behavior

```csharp
// After this step:
group.MapGet("{id}", async (Handler handler, Guid id) => await handler.GetAsync(id));
```

## Why This Matters

While minimal APIs handle unawaited tasks correctly at runtime, exceptions inside async handlers produce faulted tasks rather than propagating synchronously. This causes subtle differences in exception middleware behavior. Generating `await` matches what a developer would write by hand.

## Files to Modify

| File | Action |
|------|--------|
| `src/Nuons.Http.Generators/EndpointIncrement.cs` | **Modify** - add `IsAsync` flag to `EndpointRegistration` |
| `src/Nuons.Http.Generators/EndpointGenerator.cs` | **Modify** - detect async return types |
| `src/Nuons.Http.Generators/EndpointSourceBuilder.cs` | **Modify** - emit `async`/`await` |
| `tests/Nuons.Http.Generators.Tests/Samples.cs` | **Modify** - add async handler methods |
| `tests/Nuons.Http.Generators.Tests/EndpointGeneratorTests.EndpointExtensionsAreGeneratedCorrectly.verified.txt` | **Update** |

## Implementation Details

### 1. Add `IsAsync` Flag to `EndpointRegistration` in `EndpointIncrement.cs`

```csharp
internal record EndpointRegistration(
    string HttpMethod,
    string Route,
    string HandlerMethod,
    ImmutableArray<MethodParameter> HandlerMethodParameters,
    ImmutableArray<EndpointConfigurationCall> ConfigurationCalls,
    bool IsAsync);
```

### 2. Detect Async Return Types in `EndpointGenerator.cs`

In the `TryCreateEndpoint` method (second overload), after extracting the method symbol, check its return type:

```csharp
private static bool IsAsyncReturnType(IMethodSymbol method)
{
    var returnType = method.ReturnType;
    var fullName = returnType.OriginalDefinition.ToDisplayString();

    return fullName is
        "System.Threading.Tasks.Task" or
        "System.Threading.Tasks.Task<TResult>" or
        "System.Threading.Tasks.ValueTask" or
        "System.Threading.Tasks.ValueTask<TResult>";
}
```

Pass the result when constructing `EndpointRegistration`:

```csharp
var isAsync = IsAsyncReturnType(method);
endpointBuilder.Add(new EndpointRegistration(httpMethod, subRoute, method.Name, parameterBuilder.ToImmutable(), configCalls, isAsync));
```

### 3. Emit `async`/`await` in `EndpointSourceBuilder.cs`

When building the lambda expression:

```csharp
// Current pattern:
// ({params}) => handler.Method({args})

// New pattern for async:
// async ({params}) => await handler.Method({args})

if (endpoint.IsAsync)
{
    sb.Append($"async ({lambdaParams}) => await handler.{endpoint.HandlerMethod}({callArgs})");
}
else
{
    sb.Append($"({lambdaParams}) => handler.{endpoint.HandlerMethod}({callArgs})");
}
```

### Return Type Detection Details

Use `OriginalDefinition` to match generic types. This handles:
- `Task` → matches `System.Threading.Tasks.Task`
- `Task<string>` → `OriginalDefinition` is `System.Threading.Tasks.Task<TResult>`
- `ValueTask` → matches `System.Threading.Tasks.ValueTask`
- `ValueTask<IResult>` → `OriginalDefinition` is `System.Threading.Tasks.ValueTask<TResult>`

Do NOT match:
- `void` → synchronous
- `string` → synchronous
- `IResult` → synchronous
- Custom awaitables → not supported (edge case, out of scope)

## Test Plan

### Add Test Samples to `Samples.cs`

Add async methods to an existing handler or create a new one:

```csharp
[Route("async-ops")]
public class AsyncHandler
{
    [Get("{id}")]
    public Task<string> GetAsync(Guid id) => Task.FromResult("");

    [Get]
    public Task<string> ListAsync() => Task.FromResult("");

    [Post]
    public Task CreateAsync(string body) => Task.CompletedTask;

    [Put("{id}")]
    public ValueTask<string> UpdateAsync(Guid id, string body) => new ValueTask<string>("");

    [Delete("{id}")]
    public ValueTask DeleteAsync(Guid id) => default;
}
```

Also ensure existing synchronous handlers remain unchanged (no `async`/`await` added).

### Add a Mixed Handler

```csharp
[Route("mixed-async")]
public class MixedAsyncHandler
{
    [Get("{id}")]
    public Task<string> GetAsync(Guid id) => Task.FromResult("");

    [Get]
    public string List() => "";

    [Post]
    public Task CreateAsync(string body) => Task.CompletedTask;

    [Delete("{id}")]
    public void Delete(Guid id) { }
}
```

### Update Snapshot

The snapshot should show:
1. Async methods: `async (params) => await handler.MethodAsync(args)`
2. Sync methods: `(params) => handler.Method(args)` (unchanged)
3. Mixed handlers correctly distinguish between sync and async methods

### Verify

1. `Task<T>` return type generates `async`/`await`
2. `Task` (non-generic) return type generates `async`/`await`
3. `ValueTask<T>` return type generates `async`/`await`
4. `ValueTask` (non-generic) return type generates `async`/`await`
5. `void`, `string`, `IResult`, and other non-async types do NOT generate `async`/`await`
6. Async endpoints with configuration calls (from Step 4) chain correctly: `async (...) => await handler.Method(...)` followed by `.RequireAuthorization()` etc.
7. All existing synchronous handler tests pass unchanged

### Note on Test Compilation

The test compilation context needs to have `System.Threading.Tasks` types available. Since tests target `net10.0`, this is automatically available. However, verify that the `NuonGeneratorFixture` compilation includes the correct references for task types. If the test compilation is minimal, you may need to add a reference to `System.Runtime` or `System.Threading.Tasks`.

## Documentation Update

Add a note to the "Generation Logic" section in `documentation/nuons-http.md`:

```markdown
### Async Support

Handler methods that return `Task`, `Task<T>`, `ValueTask`, or `ValueTask<T>` are automatically detected.
The generator emits `async` lambdas with `await` for these methods:

```csharp
group.MapGet("{id}", async (Handler handler, Guid id) => await handler.GetAsync(id));
```

Synchronous methods continue to generate standard (non-async) lambdas.
```

## Build & Test Commands

```bash
dotnet build src/Nuons.Http.Generators/Nuons.Http.Generators.csproj
dotnet test tests/Nuons.Http.Generators.Tests/Nuons.Http.Generators.Tests.csproj
```
