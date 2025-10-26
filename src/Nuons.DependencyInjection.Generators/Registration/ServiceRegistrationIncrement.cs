using System.Collections.Immutable;

namespace Nuons.DependencyInjection.Generators.Registration;

internal record ServiceRegistrationIncrement(string? AssemblyName, ImmutableArray<ServiceLifetimeRegistration?> Registrations);
