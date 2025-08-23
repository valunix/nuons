using Microsoft.CodeAnalysis;
using Nuons.DependencyInjection.Generators.Registration;

namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class CombinedRegistrationGeneratorFixture : GeneratorFixture
{
	protected override IIncrementalGenerator NewGenerator() => new CombinedRegistrationGenerator();
}
