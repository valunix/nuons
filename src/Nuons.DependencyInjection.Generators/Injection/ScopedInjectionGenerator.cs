using Microsoft.CodeAnalysis;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators.Injection;

[Generator]
internal class ScopedInjectionGenerator : InjectionGenerator<ScopedAttribute>;
