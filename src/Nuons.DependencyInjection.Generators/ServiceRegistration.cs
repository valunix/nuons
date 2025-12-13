using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators;

record ServiceRegistration(string ServiceType, string ImplementingType, Lifetime Lifetime);
