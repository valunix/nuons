using Microsoft.CodeAnalysis;
using Nuons.DependencyInjection.Generators.Registration;

namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class RootRegistrationGeneratorFixture : GeneratorFixture
{
	protected override IIncrementalGenerator NewGenerator() => new RootRegistrationGenerator();
}
