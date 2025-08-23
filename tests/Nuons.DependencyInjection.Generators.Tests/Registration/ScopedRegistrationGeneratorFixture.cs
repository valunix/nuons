using Microsoft.CodeAnalysis;
using Nuons.DependencyInjection.Generators.Registration;

namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class ScopedRegistrationGeneratorFixture : GeneratorFixture
{
	protected override IIncrementalGenerator NewGenerator() => new ScopedRegistrationGenerator();
}
