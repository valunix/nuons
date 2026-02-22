# Nuons Dependency Injection

This document covers the design for Nuons dependency injection generators.

## Goal

Simplify service and options registration in the dependency injection container.
Clients only mark relevant services and Nuons takes care of the rest.
Clients will register all services from a single extension point.

## Service Registration Design

Clients will mark classes with one of three attributes, one for each lifetime:

- `Singleton`
- `Scoped`
- `Transient`

Source generators will generate boilerplate code that registers services.
The generator will generate a single extension method that generates registrations for all services in that assembly.
Generated code will be in the `Nuons.DependencyInjection.Extensions` namespace.
The assembly name will be used to generate a unique class name for each assembly.
The combined generator will generate code that calls registrations from all referenced assemblies so that clients have a single point of access.

### Service Registration Type

When registering services, we provide a service type and an implementation type.
The implementation type is the class that is marked.
The service type is determined based on two types of attributes.

When a parameterized attribute is used (e.g., `Singleton<IMyService>`), then the parameter type is used as the service type (`IMyService` in the given example).

If a plain attribute is used, then it depends on the class implementation:

- If the class implements only a single interface directly, then the type of that interface is used.
- If the class implements multiple interfaces directly or doesn't implement any interfaces, then the type of that class is used as the service type (same as the implementation type).

### Options (i.e. Configuration) Registration Design

Works on the same principles as service registration.

Target classes are marked with the `Options` attribute with a string parameter which will be used to get the configuration section.
Nuons will generate code that will register the options class.
