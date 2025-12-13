using System.Collections.Immutable;

namespace Nuons.DependencyInjection.Generators;

internal record ServiceRegistrationIncrement(string? AssemblyName, ImmutableArray<ServiceRegistration> Registrations);
