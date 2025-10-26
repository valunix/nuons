using System.Collections.Immutable;

namespace Nuons.DependencyInjection.Generators.Registration;

internal record RegistrationIncrement(string? AssemblyName, ImmutableArray<ServiceRegistration?> Registrations);
