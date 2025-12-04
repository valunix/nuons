using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Generators.Tests.Injection;

public class InjectConstructorGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact]
	public void DevRunGenerator() => fixture.RunGenerator<InjectConstructorGenerator>(output);

	[Fact]
	public Task InjectedConstructorIsGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<InjectConstructorGenerator>();
		return Verify(sources);
	}
}
