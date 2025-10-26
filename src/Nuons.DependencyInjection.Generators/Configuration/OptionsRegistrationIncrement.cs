using System.Collections.Immutable;

namespace Nuons.DependencyInjection.Generators.Configuration;

internal record OptionsRegistrationIncrement(string? AssemblyName, ImmutableArray<OptionsRegistration?> Registrations);
