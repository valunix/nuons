# Nuons Code Injection

This document covers the design for Nuons code injection generators.

## Goal

Create source generators that will generate different kinds of boiler plate code that can be seen as enriching target classes.
Current implementations:

- Constructor injection

Future considerations

- DTO classes
- DTO mappings

## Constructor injection goal

Simplify constructor injection by generating boilerplate code for constructor and field assignment.
Clients only mark relevant classes and fields, and Nuons takes care of generating the required constructor.

### Constructor Injection design

To enable code injection for a class, it must be marked with the `InjectConstructor` attribute.
The class must be partial so that the generator can add the constructor in a separate file.

Fields need to be marked for injection with one of these two attributes:

- `Injected`: Used for standard service and options injection.
- `InjectedOptions`: Used for unpacked options injection (i.e. using `private readonly MyOptions options;` instead of `private readonly IOptions<MyOptions> options;`).

### Constructor Injection generation Logic

The source generator scans for classes marked with `InjectConstructor` and looks for fields marked with `Injected` or `InjectedOptions`.
It then generates a partial class containing a constructor that:

1. Accepts all marked fields as parameters.
2. Assigns these parameters to the corresponding fields.

If a class is marked with `InjectConstructor` but has no `Injected` or `InjectedOptions` fields, an empty parameterless constructor (`public Foo() { }`) is generated. Marking the class is treated as the intent to generate a constructor, so the output stays consistent regardless of field count.

### Supported class shapes

- Top-level classes, including generic classes (e.g. `Foo<T>`), are supported.
- Classes in the **global namespace** (no namespace declaration) are supported — the generated partial is emitted at the top level without a namespace.
- **Nested classes are not supported.** A class declared inside another type is skipped — no constructor is generated for it, and the `NUCI007` analyzer reports a warning so the skip is not silent.
