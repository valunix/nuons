using Microsoft.CodeAnalysis;

namespace Nuons.DependencyInjection.Generators.Injection;

[Generator]
internal class TransientInjectionGenerator : InjectionGenerator<TransientAttribute> {}
