using Microsoft.CodeAnalysis;
using Nuons.DependencyInjection.Generators.Configuration;

namespace Nuons.DependencyInjection.Generators.Tests.Configuration;

public class OptionsRegistrationGeneratorFixture : GeneratorFixture
{
	protected override IIncrementalGenerator NewGenerator() => new OptionsRegistrationGenerator();
}
