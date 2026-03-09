# Step 1: Custom Nuons Parameter Binding Attributes

## Goal

Add parameter binding attributes to `Nuons.Http.Abstractions` and update the generator to emit corresponding ASP.NET Core attributes in the generated lambda parameters.

Currently, the generator copies parameter type and name but drops all attributes (`EndpointGenerator.cs:184-188`). This causes incorrect parameter binding in generated minimal API lambdas.

## Design Context

Handler projects target `netstandard2.0` and cannot reference ASP.NET Core packages. Nuons already uses custom attributes (`[Get]`, `[Post]`, `[Route]`) that the generator translates to ASP.NET constructs. Parameter binding attributes follow the same pattern.

## Files to Modify

| File | Action |
|------|--------|
| `src/Nuons.Http.Abstractions/FromHeaderAttribute.cs` | **Create** |
| `src/Nuons.Http.Abstractions/FromQueryAttribute.cs` | **Create** |
| `src/Nuons.Http.Abstractions/FromBodyAttribute.cs` | **Create** |
| `src/Nuons.Http.Abstractions/FromRouteAttribute.cs` | **Create** |
| `src/Nuons.Http.Abstractions/FromFormAttribute.cs` | **Create** |
| `src/Nuons.Http.Abstractions/FromServicesAttribute.cs` | **Create** |
| `src/Nuons.Http.Abstractions/AsParametersAttribute.cs` | **Create** |
| `src/Nuons.Http.Generators/KnownHttpTypes.cs` | **Modify** - add constants |
| `src/Nuons.Http.Generators/EndpointIncrement.cs` | **Modify** - extend `MethodParameter` |
| `src/Nuons.Http.Generators/EndpointGenerator.cs` | **Modify** - read attributes from parameters |
| `src/Nuons.Http.Generators/EndpointSourceBuilder.cs` | **Modify** - emit ASP.NET attributes |
| `tests/Nuons.Http.Generators.Tests/Samples.cs` | **Modify** - add test handlers |
| `tests/Nuons.Http.Generators.Tests/EndpointGeneratorTests.EndpointExtensionsAreGeneratedCorrectly.verified.txt` | **Update** - snapshot |

## Implementation Details

### 1. Create Attribute Classes in `src/Nuons.Http.Abstractions/`

Follow the existing pattern from `GetAttribute.cs`. All attributes use `[AttributeUsage(AttributeTargets.Parameter)]`.

**FromHeaderAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class FromHeaderAttribute : Attribute
{
    public FromHeaderAttribute(string name = "") { }
}
```

**FromQueryAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class FromQueryAttribute : Attribute
{
    public FromQueryAttribute(string name = "") { }
}
```

**FromBodyAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class FromBodyAttribute : Attribute
{
    public FromBodyAttribute() { }
}
```

**FromRouteAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class FromRouteAttribute : Attribute
{
    public FromRouteAttribute(string name = "") { }
}
```

**FromFormAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class FromFormAttribute : Attribute
{
    public FromFormAttribute(string name = "") { }
}
```

**FromServicesAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class FromServicesAttribute : Attribute
{
    public FromServicesAttribute() { }
}
```

**AsParametersAttribute.cs:**
```csharp
using System;

namespace Nuons.Http.Abstractions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class AsParametersAttribute : Attribute
{
    public AsParametersAttribute() { }
}
```

### 2. Add Constants to `KnownHttpTypes.cs`

Add these constants after the existing `DeleteAttribute` constant:

```csharp
public const string FromHeaderAttribute = "Nuons.Http.Abstractions.FromHeaderAttribute";
public const string FromQueryAttribute = "Nuons.Http.Abstractions.FromQueryAttribute";
public const string FromBodyAttribute = "Nuons.Http.Abstractions.FromBodyAttribute";
public const string FromRouteAttribute = "Nuons.Http.Abstractions.FromRouteAttribute";
public const string FromFormAttribute = "Nuons.Http.Abstractions.FromFormAttribute";
public const string FromServicesAttribute = "Nuons.Http.Abstractions.FromServicesAttribute";
public const string AsParametersAttribute = "Nuons.Http.Abstractions.AsParametersAttribute";
```

### 3. Extend `MethodParameter` Record in `EndpointIncrement.cs`

Add a binding attribute field. Use a nullable string to represent the optional ASP.NET attribute to emit:

```csharp
// Replace:
internal record MethodParameter(string Type, string Name);

// With:
internal record ParameterBindingAttribute(string AspNetAttribute, string? Name);
internal record MethodParameter(string Type, string Name, ParameterBindingAttribute? BindingAttribute = null);
```

The `AspNetAttribute` is the fully-qualified ASP.NET attribute type (e.g., `Microsoft.AspNetCore.Mvc.FromHeaderAttribute`). The `Name` is the optional name argument.

### 4. Read Attributes in `EndpointGenerator.cs`

In the `TryCreateEndpoint` method (second overload, lines 182-189), update the parameter extraction loop to check for Nuons binding attributes:

```csharp
foreach (var parameter in parameters)
{
    var typeName = parameter.Type.ToFullTypeName();
    var name = parameter.Name;
    var bindingAttribute = GetBindingAttribute(parameter);
    parameterBuilder.Add(new MethodParameter(typeName, name, bindingAttribute));
}
```

Add a helper method:

```csharp
private static ParameterBindingAttribute? GetBindingAttribute(IParameterSymbol parameter)
{
    foreach (var attr in parameter.GetAttributes())
    {
        var fullName = attr.AttributeClass?.ToDisplayString();
        switch (fullName)
        {
            case KnownHttpTypes.FromHeaderAttribute:
                return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromHeaderAttribute", GetNameArg(attr));
            case KnownHttpTypes.FromQueryAttribute:
                return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromQueryAttribute", GetNameArg(attr));
            case KnownHttpTypes.FromBodyAttribute:
                return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromBodyAttribute", null);
            case KnownHttpTypes.FromRouteAttribute:
                return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromRouteAttribute", GetNameArg(attr));
            case KnownHttpTypes.FromFormAttribute:
                return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromFormAttribute", GetNameArg(attr));
            case KnownHttpTypes.FromServicesAttribute:
                return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromServicesAttribute", null);
            case KnownHttpTypes.AsParametersAttribute:
                return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.AsParametersAttribute", null);
        }
    }
    return null;
}

private static string? GetNameArg(AttributeData attr)
{
    if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string s && !string.IsNullOrEmpty(s))
        return s;
    return null;
}
```

### 5. Emit ASP.NET Attributes in `EndpointSourceBuilder.cs`

When building lambda parameter strings, prepend the ASP.NET attribute if present:

```csharp
// When building a parameter string:
if (param.BindingAttribute is { } binding)
{
    if (binding.Name is not null)
        sb.Append($"[global::{binding.AspNetAttribute}(Name = \"{binding.Name}\")] ");
    else
        sb.Append($"[global::{binding.AspNetAttribute}] ");
}
sb.Append($"{param.Type} {param.Name}");
```

### Expected Generated Output

```csharp
app.MapGet("/example/{id}", (Handler handler, Guid id, [global::Microsoft.AspNetCore.Http.FromHeaderAttribute(Name = "Authorization")] string authorization, [global::Microsoft.AspNetCore.Http.FromQueryAttribute] int page) => handler.Get(id, authorization, page));
```

## Test Plan

### Add Test Samples

Add a new handler class to `tests/Nuons.Http.Generators.Tests/Samples.cs`:

```csharp
[Route("binding-test")]
public class BindingTestHandler
{
    [Get("{id}")]
    public string GetWithBindings(
        [FromRoute] Guid id,
        [FromHeader("Authorization")] string auth,
        [FromQuery] int page,
        [FromQuery("page_size")] int pageSize)
    {
        return "";
    }

    [Post]
    public void CreateWithBody([FromBody] CreateRequest request, [FromServices] ILogger logger)
    {
        return;
    }

    [Post("form")]
    public void CreateFromForm([FromForm] string name, [AsParameters] FormParams formParams)
    {
        return;
    }
}
```

You will also need placeholder types (`CreateRequest`, `FormParams`, `ILogger`) or use existing types.

### Update Snapshot Tests

Run the existing test `EndpointExtensionsAreGeneratedCorrectly` and accept the new snapshot. The snapshot file at `tests/Nuons.Http.Generators.Tests/EndpointGeneratorTests.EndpointExtensionsAreGeneratedCorrectly.verified.txt` should now include the generated code for `BindingTestHandler` with ASP.NET attributes.

### Verify

1. All existing tests still pass (no regressions from the `MethodParameter` record change)
2. The new handler generates correct attribute annotations
3. Parameters without binding attributes still work as before (no attribute emitted)
4. Named attributes emit `(Name = "value")` syntax
5. Nameless attributes emit just the attribute without `Name` property

## Build & Test Commands

```bash
dotnet build src/Nuons.Http.Generators/Nuons.Http.Generators.csproj
dotnet build src/Nuons.Http.Abstractions/Nuons.Http.Abstractions.csproj
dotnet test tests/Nuons.Http.Generators.Tests/Nuons.Http.Generators.Tests.csproj
```

## Documentation Update

Update `documentation/nuons-http.md` to add a new section after "Endpoint Registration Design" describing parameter binding attributes:

```markdown
## Parameter Binding Attributes

Method parameters can be annotated with binding attributes to control how ASP.NET Core binds request data to parameters.
The following attributes are available in `Nuons.Http.Abstractions`:

- `FromHeader(name)`: Binds from an HTTP header. Optional `name` parameter overrides the header name.
- `FromQuery(name)`: Binds from a query string parameter. Optional `name` parameter overrides the query key.
- `FromBody`: Binds from the request body.
- `FromRoute(name)`: Binds from a route parameter. Optional `name` parameter overrides the route segment name.
- `FromForm(name)`: Binds from form data. Optional `name` parameter overrides the form field name.
- `FromServices`: Binds from the dependency injection container.
- `AsParameters`: Binds a complex object's properties as individual parameters.

These are Nuons-specific attributes (not ASP.NET Core attributes) because handler projects target `netstandard2.0`.
The generator translates them to the corresponding `Microsoft.AspNetCore.Http` attributes in the generated code.
Parameters without a binding attribute use ASP.NET Core's default binding behavior.
```

## Notes

- The `MethodParameter` default value `BindingAttribute = null` ensures backward compatibility — existing code that creates `MethodParameter(type, name)` continues to work.
- The handler class itself (the first parameter in the lambda) will never have a binding attribute, so the logic correctly skips it.
