using System.Collections.Immutable;

namespace Nuons.DependencyInjection.Generators.Registration;

internal record RootRegistrationIncrement(string namespaceName, string ClassName, ImmutableArray<string> Registrations);