# Nuons solution architecture

## Architectural decisions

- Usage must be as simple as possible, goal for clients:
    1. Add package
    2. Mark code with attributes
    3. Profit
- Minimize number of packages clients need to add.
- Whenever possible, the source generator will create a file and generate code, even if the contents are empty.
  - Reasoning: easier to debug, supports chaining generators
- When possible generated code will be in `Nuons` namespace to avoid conflicts.

TODO others?

## Project structure

Solution will be broken into multiple projects.
Err on the side of having more projects than less.

TODO alternative consideration: combine everything into fewer number of projects? Could simplify packaging.

Naming convention: Nuons.{area}.{project-type}

- area
  - group of generators that work towards the same goal
  - could be a candiate for standalone usage
- project-type - Suffix to indicate project contents, one of:
  - Abstractions
    - shared code
    - contains marker attributes
    - referenced and used in client code
  - Generators
    - source generators for the given area that are assembly scoped
  - Generators.Startup
    - source generators for the given area that are startup dependent
    - added to startup projects only
  - Analyzers
    - analyzers for the given area that support easier usage of generators
  - Analyzers.Startup (TODO: do we need this one?)
    - analyzers for the given area that support easier usage of generators
    - added to startup projects only

Areas:

- [Dependency Injection](nuons-dependency-injection.md)
- [Code Injection](nuons-code-injection.md)
- [HTTP](nuons-http.md)

## Generators design

There are two types of generators in Nuons:

1. Assembly scoped generators:
   - Standard source generators as envisioned - generate and enrich code in a given assembly (e.g. constructor injection).
   - Works only within given assembly.
   - Doesn't depend on other generators, or is used by other generators.
   - No special architectural design considerations.
2. Startup dependent generators
   - Generator aims to generate code based on multiple assemblies (e.g., service registration)
   - Generated code is used in application startup

### Startup dependent generators

Current design:

Use two generators to implement single functionality.
First generator generates code within a single assembly and is scoped to it.
It always generates predefined code that the other generator relies on.
Second generator is run only in startup projects.
It scans referenced assemblies (and its own assembly) and generates code that combines outputs.

Pros:

- Relatively aligned with the intent for source generators to "live" within a single assembly
- Cleaner separation of responsibility
- Supports internal classes directly
- Assumption is that it will scale better on larger solutions
  - TODO: test if there is measurable impact

Cons:

- More complexity due to 2 generators
- Risk of breaking the chain of code if the first generator fails to generate code

TODO consider alternatives:

1. Single generator that generates all relevant code in startup
   - Single generator only
   - Simpler setup
   - Doesn't support internal classes directly (if they have to be referenced in startup project)
   - Can't leverage `ForAttributeWithMetadataName` approach

### Marking code

Generators will use attributes to mark relevant code so that `ForAttributeWithMetadataName` approach can be leveraged whenever possible.

When depending on code from multiple assemblies, `AssemblyHasNuonsAttribute` will be used to mark relevant assemblies.
Current approach: clients have to manually mark assemblies.
Desired approach: automatically added by the generator.

Marker attributes will be created and packed as "standard" code in "Abstractions" projects.
Clients will get them by using a NuGet package.
Generators will use the same marker attributes as those provided to the clients (no hardcoded strings).

## Packaging

To fully support the architectural goal/decision of minimizing user effort, the number of packages that clients need to use must be minimized.

Typical approaches:

1. Single package
   - works for simple cases where generators enrich current assembly only
2. Generators + Abstractions combination
   - popular combination for slightly more complex scenarios where code is generated based on multiple assemblies

Current approach for Nuons:

Two packages: `Nuons` and `Nuons.Startup`.
`Nuons` is added to every project that needs code generation.
`Nuons.Startup` is added to the startup project only.
Both packages contain both generators and abstractions.
`Nuons.Startup` will contain all generators from the `Nuons` package (to cover the current assembly) and, in addition to that, "startup specific" generators.

Project file configuration will be used to define the NuGet package until it either becomes too complex or cannot achieve the desired functionality.

### NuGet package structure

NuGet packages have 2 relevant folders where DLLs are placed:

1. `lib/netstandard2.0`
   - "Standard" lib directory used by projects
   - Abstractions DLLs have to go here so that marker attributes are available to clients
2. `analyzers/dotnet/cs/`
   - Directory for analyzers and generators (since they are built with the same mechanism)
   - Analyzers, Generators, and Abstractions will go here.
     - Important to include Abstractions as otherwise they will not be available to Generators due to how they are loaded and executed.

### Packaging approach

The current approach to packaging is to have `Nuons` and `Nuons.Startup` as empty projects which reference other relevant projects.
Building the NuGet package is defined manually in the project file, indicating which DLLs to include where.
The result is a single NuGet package with multiple DLLs within it.

This approach is good enough for now as it allows for breaking the Nuons solution into multiple projects while still building a single NuGet package in the end.

Current downside: nuon packages must be marked with `PrivateAssets="all"` to work correctly. TODO how to achieve this automatically? Or how to package differently to avoid this.

TODO Future considerations: how to best align with the common best practice of one DLL per package? How to do this while having abstractions both as generator and client code references? Can we do it without packaging each current project as a standalone NuGet?
