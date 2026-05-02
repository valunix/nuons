# HTTP Generators Improvement Plan

This document defines the implementation steps for improving the Nuons HTTP generators.
Each step is self-contained and should be implemented and tested independently.

## Step 1: Custom Nuons parameter binding attributes

**Status:** Not started

**Problem:**
The generator copies parameter type and name but drops all attributes (`EndpointGenerator.cs:184-188`).
This causes incorrect parameter binding in the generated minimal API lambdas.

**Design decision:**
Use custom Nuons attributes in `Nuons.Http.Abstractions` instead of ASP.NET attributes.
This is consistent with the existing pattern — `[Get]`, `[Post]`, `[Route]` are already custom Nuons types,
not ASP.NET types. Handler projects target netstandard2.0 and must not depend on ASP.NET Core packages.
The generator translates Nuons attributes to ASP.NET attributes in the generated code.

**Attributes to create in `Nuons.Http.Abstractions`:**

- `FromHeaderAttribute(string name = "")`
- `FromQueryAttribute(string name = "")`
- `FromBodyAttribute`
- `FromRouteAttribute(string name = "")`
- `FromFormAttribute(string name = "")`
- `FromServicesAttribute`
- `AsParametersAttribute`

All with `[AttributeUsage(AttributeTargets.Parameter)]`.

**Implementation scope:**

- Create the attribute classes listed above
- Add attribute type constants to `KnownHttpTypes.cs`
- Read attributes from `IParameterSymbol.GetAttributes()` during parameter extraction
- Extend `MethodParameter` record to include parameter attributes
- In `EndpointSourceBuilder`, emit the corresponding ASP.NET attribute (fully qualified with `global::` prefix) before the parameter type in the generated lambda
- When the Nuons attribute has a non-empty name, emit the `Name` property on the ASP.NET attribute
- Update snapshot tests to verify attribute preservation

**Expected generated output:**
```csharp
(Handler handler, Guid id, [global::Microsoft.AspNetCore.Http.FromHeaderAttribute(Name = "Authorization")] string authorization, [global::Microsoft.AspNetCore.Http.FromQueryAttribute] int page) => handler.Get(id, authorization, page)
```

---

## Step 2: Add Patch attribute

**Status:** Not started

**Problem:**
Only GET, POST, PUT, DELETE are supported. PATCH is missing despite `HttpMethods.cs` already defining a `Patch` constant.
PATCH is widely used for partial updates and is a standard HTTP method.

**Reasoning:**
Trivial effort for an obvious gap. Leaving it out forces users to hand-write `MapPatch` calls, breaking the "all endpoints generated" experience.

**Implementation scope:**
- Create `PatchAttribute.cs` in `Nuons.Http.Abstractions`
- Add `Patch` to `KnownHttpTypes`
- Add `Patch` to `EndpointAttributes` record and its `FromCompilation` factory
- Add patch attribute check in `TryCreateEndpoint` (this is also a good time to address the existing `// TODO foreach on endpoint spec instead of 4 IFs` at line 124)
- Add test samples and update snapshot tests

---

## Step 3: Generate MapGroup() instead of route string concatenation

**Status:** Not started

**Problem:**
Currently, `RouteBuilder` concatenates the class-level `[Route]` prefix with the method-level sub-route into a flat string.
Each endpoint is registered independently with the full route. This means there's no way to apply shared configuration (auth, CORS, rate limiting, tags) to all endpoints in a handler class.

**Reasoning:**
MapGroup by itself is cosmetic — the generated code isn't read by humans. The real value is that it's a prerequisite for Step 4: class-level attributes like `[Authorize]` need to apply to all endpoints in a handler, which maps naturally to `group.RequireAuthorization()`. Without MapGroup, Step 4 would require emitting per-endpoint configuration calls redundantly.

**Implementation scope:**
- Update `EndpointSourceBuilder` to generate `MapGroup()` per handler class
- Methods within a handler become `group.MapGet(subRoute, ...)` instead of `app.MapGet(fullRoute, ...)`
- `RouteBuilder` is no longer needed for concatenation; base route goes to group, sub-route goes to method
- Update snapshot tests

**Expected generated output:**
```csharp
var domainObjectGroup = app.MapGroup("/domain-object");
domainObjectGroup.MapGet("{id}", (Handler handler, string id) => handler.Get(id));
domainObjectGroup.MapPost("", (Handler handler, DomainObject obj) => handler.Create(obj));
domainObjectGroup.MapDelete("{id}", (Handler handler, Guid id) => handler.Delete(id));
```

---

## Step 4: Custom Nuons attributes for endpoint configuration

**Status:** Not started

**Problem:**
Real-world endpoints need authorization, OpenAPI metadata, rate limiting, etc.
Currently there is no way to express these through the generator.

**Design decision:**
Use custom Nuons attributes in `Nuons.Http.Abstractions`, same as Step 1.
Handler projects cannot reference ASP.NET Core packages, so standard attributes like `[Authorize]` are not available.
The generator translates Nuons attributes to the corresponding minimal API configuration calls.
With Step 3 (MapGroup) in place, attributes on the handler class apply to the group, and attributes on methods apply to individual endpoints.

**Attributes to create in `Nuons.Http.Abstractions`:**

Phase A - Authorization:

- `AuthorizeAttribute(string policy = "")` — `[AttributeUsage(Class | Method)]`
- `AllowAnonymousAttribute` — `[AttributeUsage(Method)]`

Phase B - OpenAPI metadata:

- `TagsAttribute(params string[] tags)` — `[AttributeUsage(Class | Method)]`
- `EndpointNameAttribute(string name)` — `[AttributeUsage(Method)]`
- `EndpointDescriptionAttribute(string description)` — `[AttributeUsage(Method)]`

Phase C - Other (as needed):

- `EnableRateLimitingAttribute(string policy)` — `[AttributeUsage(Class | Method)]`
- `RequireCorsAttribute(string policy)` — `[AttributeUsage(Class | Method)]`
- `DisableAntiforgeryAttribute` — `[AttributeUsage(Class | Method)]`

**Generator translation mapping:**

Phase A:

- `[Authorize]` on class → `group.RequireAuthorization()`
- `[Authorize("PolicyName")]` on class → `group.RequireAuthorization("PolicyName")`
- `[Authorize]` on method → `.RequireAuthorization()`
- `[Authorize("PolicyName")]` on method → `.RequireAuthorization("PolicyName")`
- `[AllowAnonymous]` on method → `.AllowAnonymous()`

Phase B:

- `[Tags("tag")]` → `.WithTags("tag")`
- `[EndpointName("name")]` → `.WithName("name")`
- `[EndpointDescription("desc")]` → `.WithDescription("desc")`

Phase C:

- `[EnableRateLimiting("policy")]` → `.RequireRateLimiting("policy")`
- `[RequireCors("policy")]` → `.RequireCors("policy")`
- `[DisableAntiforgery]` → `.DisableAntiforgery()`

**Design consideration:**
Each phase should be its own PR. The generator should only emit configuration calls for attributes it explicitly recognizes — unknown attributes are silently ignored (not an error). This keeps forward compatibility as new phases are added.

---

## Step 5: Add await for async handler methods

**Status:** Not started

**Problem:**
The generated lambda calls `handler.Method(params)` without `await`, even for methods returning `Task<T>` or `ValueTask<T>`.
While minimal APIs handle unawaited tasks correctly at runtime, exceptions inside async handlers produce faulted tasks rather than propagating synchronously. This can cause subtle differences in exception middleware behavior.

**Reasoning:**
Low effort, small improvement. Makes the generated code match what a developer would write by hand.

**Implementation scope:**
- During method extraction, check if the return type is `Task`, `Task<T>`, `ValueTask`, or `ValueTask<T>`
- If async, generate `async (params) => await handler.Method(params)` instead of `(params) => handler.Method(params)`
- Update snapshot tests

**Expected generated output:**
```csharp
group.MapGet("{id}", async (Handler handler, Guid id) => await handler.Get(id));
```

---

## Out of scope (intentionally excluded)

These were considered and intentionally left out to preserve simplicity:

- **Custom configuration callbacks / interfaces** - An interface (e.g. `IEndpointConfiguration`) or static method convention that lets handlers write arbitrary imperative configuration code against `RouteGroupBuilder`/`RouteHandlerBuilder`. Would cover 100% of minimal API features (endpoint filters, complex OpenAPI lambdas, conditional logic) without generator changes. Excluded because it requires handler projects to reference ASP.NET Core types (breaking netstandard2.0 separation), mixes declarative and imperative styles, and the use cases it covers (custom filters, advanced OpenAPI) are rare enough that hand-writing those few endpoints is acceptable.
- **Handler lifetime attributes (`[Transient]`, `[Singleton]`)** - Scoped is the correct default for HTTP handlers in virtually all cases. Adding options creates a decision point with no real benefit.
- **Covering every minimal API feature** - The 80/20 rule applies. The generator should handle common cases cleanly and let users fall back to manual `MapGet`/`MapPost` for edge cases.
