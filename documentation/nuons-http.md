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

Each of these attributes can take an optional string parameter to specify a sub-route.

## Generation Logic

The source generator scans for classes marked with `Route` and methods marked with `Get`, `Post`, `Put`, or `Delete`.
It then generates a static class `NuonEndpointExtensions` in the `Nuons.Http.Extensions` namespace containing two extension methods:

1. `AddNuonHttpServices(this IServiceCollection services)`: Registers all handler classes as scoped services in the dependency injection container.
2. `MapNuonEndpoints(this IEndpointRouteBuilder app)`: Maps the discovered methods to their corresponding routes using ASP.NET Core's Minimal APIs (e.g., `app.MapGet(...)`).

### Route Building

The final route for an endpoint is a combination of the base route defined in the `Route` attribute and the sub-route defined in the method attribute.

## Multi-Assembly Support

The HTTP generator supports discovering endpoints across multiple assemblies.
Assemblies that contain Nuons-marked code should be marked with the `AssemblyHasNuons` attribute.
The generator scans referenced assemblies for classes with the `Route` attribute and includes them in the generated registration methods.

To support native AOT all endpoint registrations must be in the startup project.
This constraints the design into single generator in startup project only that collects handlers from referenced assemblies.
Additional constraint is that currently we don't support handlers amrked as `internal`. Future consideration: support it via public proxy class generated in the referenced assembly.
