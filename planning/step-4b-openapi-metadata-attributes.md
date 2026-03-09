# Step 4B: OpenAPI Metadata Attributes

## Goal

Add `[Tags]`, `[EndpointName]`, and `[EndpointDescription]` attributes to `Nuons.Http.Abstractions` and update the generator to emit `.WithTags()`, `.WithName()`, and `.WithDescription()` configuration calls.

**Prerequisite:** Step 4A (Authorization) must be completed first, as it introduces the `EndpointConfigurationCall` infrastructure.

## Translation Mapping

| Nuons Attribute | Target | Generated Code |
|----------------|--------|----------------|
| `[Tags("tag1", "tag2")]` on class | Group | `group.WithTags("tag1", "tag2")` |
| `[Tags("tag")]` on method | Endpoint | `.WithTags("tag")` |
| `[EndpointName("name")]` on method | Endpoint | `.WithName("name")` |
| `[EndpointDescription("desc")]` on method | Endpoint | `.WithDescription("desc")` |

## Files to Modify

| File | Action |
|------|--------|
| `src/Nuons.Http.Abstractions/TagsAttribute.cs` | **Create** |
| `src/Nuons.Http.Abstractions/EndpointNameAttribute.cs` | **Create** |
| `src/Nuons.Http.Abstractions/EndpointDescriptionAttribute.cs` | **Create** |
| `src/Nuons.Http.Generators/KnownHttpTypes.cs` | **Modify** - add constants |
| `src/Nuons.Http.Generators/EndpointIncrement.cs` | **Modify** - update `EndpointConfigurationCall` if needed |
| `src/Nuons.Http.Generators/EndpointGenerator.cs` | **Modify** - extract OpenAPI attributes |
| `src/Nuons.Http.Generators/EndpointSourceBuilder.cs` | **Modify** - handle multi-argument config calls |
| `tests/Nuons.Http.Generators.Tests/Samples.cs` | **Modify** - add test handlers |
| `tests/Nuons.Http.Generators.Tests/EndpointGeneratorTests.EndpointExtensionsAreGeneratedCorrectly.verified.txt` | **Update** |

## Implementation Details

### 1. Create Attribute Classes

**TagsAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class TagsAttribute : Attribute
{
    public TagsAttribute(params string[] tags) { }
}
```

**EndpointNameAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EndpointNameAttribute : Attribute
{
    public EndpointNameAttribute(string name) { }
}
```

**EndpointDescriptionAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EndpointDescriptionAttribute : Attribute
{
    public EndpointDescriptionAttribute(string description) { }
}
```

### 2. Add Constants to `KnownHttpTypes.cs`

```csharp
public const string TagsAttribute = "Nuons.Http.Abstractions.TagsAttribute";
public const string EndpointNameAttribute = "Nuons.Http.Abstractions.EndpointNameAttribute";
public const string EndpointDescriptionAttribute = "Nuons.Http.Abstractions.EndpointDescriptionAttribute";
```

### 3. Update `EndpointConfigurationCall` if Needed

The `Tags` attribute takes multiple string arguments (`params string[]`), unlike the single-argument pattern from Step 4A. Update the record to support multiple arguments:

```csharp
// Change from:
internal record EndpointConfigurationCall(string Method, string? Argument);

// To:
internal record EndpointConfigurationCall(string Method, ImmutableArray<string> Arguments);
```

Update existing Step 4A code that creates `EndpointConfigurationCall` to use the new signature:
- `RequireAuthorization("Policy")` → `new EndpointConfigurationCall("RequireAuthorization", ImmutableArray.Create("Policy"))`
- `RequireAuthorization()` → `new EndpointConfigurationCall("RequireAuthorization", ImmutableArray<string>.Empty)`
- `AllowAnonymous()` → `new EndpointConfigurationCall("AllowAnonymous", ImmutableArray<string>.Empty)`

### 4. Extract OpenAPI Attributes in `EndpointGenerator.cs`

Add to the existing class-level and method-level attribute extraction loops:

```csharp
// For Tags (class or method level):
if (fullName == KnownHttpTypes.TagsAttribute)
{
    var tags = ImmutableArray.CreateBuilder<string>();
    foreach (var arg in attr.ConstructorArguments)
    {
        if (arg.Kind == TypedConstantKind.Array)
        {
            foreach (var item in arg.Values)
            {
                if (item.Value is string s)
                    tags.Add(s);
            }
        }
        else if (arg.Value is string s)
        {
            tags.Add(s);
        }
    }
    configBuilder.Add(new EndpointConfigurationCall("WithTags", tags.ToImmutable()));
}

// For EndpointName (method level only):
if (fullName == KnownHttpTypes.EndpointNameAttribute)
{
    var name = attr.ConstructorArguments[0].Value as string;
    if (name is not null)
        configBuilder.Add(new EndpointConfigurationCall("WithName", ImmutableArray.Create(name)));
}

// For EndpointDescription (method level only):
if (fullName == KnownHttpTypes.EndpointDescriptionAttribute)
{
    var desc = attr.ConstructorArguments[0].Value as string;
    if (desc is not null)
        configBuilder.Add(new EndpointConfigurationCall("WithDescription", ImmutableArray.Create(desc)));
}
```

### 5. Update `EndpointSourceBuilder.cs` for Multi-Argument Calls

Update the configuration call emission to handle multiple arguments:

```csharp
private static void AppendConfigCall(StringBuilder sb, EndpointConfigurationCall config, string prefix = "")
{
    if (config.Arguments.IsEmpty)
    {
        sb.Append($"{prefix}.{config.Method}()");
    }
    else
    {
        var args = string.Join(", ", config.Arguments.Select(a => $"\"{a}\""));
        sb.Append($"{prefix}.{config.Method}({args})");
    }
}
```

### Expected Generated Output

```csharp
var apiHandlerGroup = app.MapGroup("/api");
apiHandlerGroup.WithTags("api", "v1");
apiHandlerGroup.MapGet("{id}", (ApiHandler handler, Guid id) => handler.Get(id)).WithName("GetApi").WithDescription("Gets an API resource");
apiHandlerGroup.MapPost("", (ApiHandler handler, CreateRequest request) => handler.Create(request)).WithTags("write-ops");
```

## Test Plan

### Add Test Samples to `Samples.cs`

```csharp
[Tags("api", "v1")]
[Route("tagged")]
public class TaggedHandler
{
    [EndpointName("GetTagged")]
    [EndpointDescription("Gets a tagged resource")]
    [Get("{id}")]
    public string Get(Guid id) => "";

    [Tags("write")]
    [Post]
    public void Create(string body) { }
}

[Route("named")]
public class NamedEndpointsHandler
{
    [EndpointName("ListItems")]
    [Get]
    public string List() => "";

    [EndpointName("GetItem")]
    [EndpointDescription("Get a single item by ID")]
    [Get("{id}")]
    public string Get(Guid id) => "";
}
```

### Update Snapshot

Verify the snapshot includes:
1. `TaggedHandler` group with `.WithTags("api", "v1")` on the group
2. GET endpoint with `.WithName("GetTagged").WithDescription("Gets a tagged resource")`
3. POST endpoint with `.WithTags("write")`
4. `NamedEndpointsHandler` with endpoint-level names and descriptions

### Verify

1. `[Tags]` on class emits `group.WithTags(...)` with all tag arguments
2. `[Tags]` on method emits `.WithTags(...)` chained on the endpoint
3. `[EndpointName]` emits `.WithName(...)` chained on the endpoint
4. `[EndpointDescription]` emits `.WithDescription(...)` chained on the endpoint
5. Multiple config calls on the same endpoint chain correctly
6. Tags with multiple arguments emit all arguments comma-separated
7. Handlers without OpenAPI attributes generate unchanged code
8. Previously added auth attributes (Step 4A) still work correctly alongside OpenAPI attributes

## Documentation Update

Add to the "Endpoint Configuration Attributes" section in `documentation/nuons-http.md` (after the Authorization subsection):

```markdown
### OpenAPI Metadata

- `Tags(params string[] tags)`: Applied to a class or method. Generates `.WithTags(...)` to categorize endpoints in OpenAPI documentation.
- `EndpointName(name)`: Applied to a method. Generates `.WithName(...)` to set the endpoint's operation ID.
- `EndpointDescription(description)`: Applied to a method. Generates `.WithDescription(...)` to set the endpoint's description.
```

## Build & Test Commands

```bash
dotnet build src/Nuons.Http.Abstractions/Nuons.Http.Abstractions.csproj
dotnet build src/Nuons.Http.Generators/Nuons.Http.Generators.csproj
dotnet test tests/Nuons.Http.Generators.Tests/Nuons.Http.Generators.Tests.csproj
```
