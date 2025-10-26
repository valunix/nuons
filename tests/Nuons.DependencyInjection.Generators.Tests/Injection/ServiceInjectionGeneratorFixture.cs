using Microsoft.CodeAnalysis;
using Nuons.DependencyInjection.Generators.Injection;

namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class ServiceInjectionGeneratorFixture : GeneratorFixture
{
	protected override IIncrementalGenerator NewGenerator() => new ServiceInjectionGenerator();
}
