using System.Collections.Immutable;

namespace Nuons.DependencyInjection.Generators;

internal record OptionsRegistrationIncrement(string? AssemblyName, ImmutableArray<OptionsRegistration> Registrations);
