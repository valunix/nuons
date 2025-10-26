using Microsoft.CodeAnalysis;
using Nuons.DependencyInjection.Generators.Injection;

namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class TransientInjectionGeneratorFixture : GeneratorFixture
{
	protected override IIncrementalGenerator NewGenerator() => new TransientInjectionGenerator();
}
