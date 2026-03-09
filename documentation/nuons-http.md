# Nuons HTTP

This document covers the design for Nuons HTTP endpoint registration generators.

## Goal

Simplify the registration of HTTP endpoints in ASP.NET Core applications.
Clients only mark their handler classes and methods with attributes, and Nuons takes care of generating the routing and service registration boilerplate.

## Endpoint Registration Design

To enable HTTP endpoint generation for a class, it must be marked with the `Route` attribute, which specifies the base route for all endpoints in that class.

Individual methods within the class are marked with one of the following HTTP method attributes to define an endpoint:

- `Get`: Maps an HTTP GET request.
- `Post`: Maps an HTTP POST request.
- `Put`: Maps an HTTP PUT request.
- `Delete`: Maps an HTTP DELETE request.
- `Patch`: Maps an HTTP PATCH request.

Each of these attributes can take an optional string parameter to specify a sub-route.

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

## Endpoint Configuration Attributes

### Authorization

- `Authorize(policy)`: Applied to a class or method. On a class, generates `group.RequireAuthorization()`. On a method, chains `.RequireAuthorization()` on the endpoint. The optional `policy` parameter specifies the authorization policy name.
- `AllowAnonymous`: Applied to a method. Chains `.AllowAnonymous()` on the endpoint, overriding class-level authorization.

These attributes are Nuons-specific (not ASP.NET Core attributes) and are translated to the corresponding minimal API configuration calls by the generator.

## Generation Logic

The source generator scans for classes marked with `Route` and methods marked with `Get`, `Post`, `Put`, `Delete`, or `Patch`.
It then generates a static class `NuonEndpointExtensions` in the `Nuons.Http.Extensions` namespace containing two extension methods:

1. `AddNuonHttpServices(this IServiceCollection services)`: Registers all handler classes as scoped services in the dependency injection container.
2. `MapNuonEndpoints(this IEndpointRouteBuilder app)`: Maps the discovered methods to their corresponding routes using ASP.NET Core's Minimal APIs (e.g., `app.MapGet(...)`).

### Route Building

Each handler class generates a `MapGroup()` call using the base route from the `Route` attribute.
Individual endpoints within the handler are registered on the group using their sub-route.

For example, a handler with `[Route("orders")]` and a method with `[Get("{id}")]` generates:

```csharp
var ordersHandlerGroup = app.MapGroup("/orders");
ordersHandlerGroup.MapGet("{id}", ...);
```

This grouping enables shared configuration (authorization, CORS, rate limiting) to be applied once at the group level.

### Async Support

Handler methods that return `Task`, `Task<T>`, `ValueTask`, or `ValueTask<T>` are automatically detected.
The generator emits `async` lambdas with `await` for these methods:

```csharp
group.MapGet("{id}", async (Handler handler, Guid id) => await handler.GetAsync(id));
```

Synchronous methods continue to generate standard (non-async) lambdas.

## Multi-Assembly Support

The HTTP generator supports discovering endpoints across multiple assemblies.
Assemblies that contain Nuons-marked code should be marked with the `AssemblyHasNuons` attribute.
The generator scans referenced assemblies for classes with the `Route` attribute and includes them in the generated registration methods.

To support native AOT all endpoint registrations must be in the startup project.
This constraints the design into single generator in startup project only that collects handlers from referenced assemblies.
Additional constraint is that currently we don't support handlers marked as `internal`. Future consideration: support it via public proxy class generated in the referenced assembly.
