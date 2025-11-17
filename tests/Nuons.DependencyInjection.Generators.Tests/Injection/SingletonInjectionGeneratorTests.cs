using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Injection;

namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class SingletonInjectionGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<SingletonInjectionGenerator>(output);

	[Fact]
	public Task SingletonInjectionIsGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<SingletonInjectionGenerator>();
		return Verify(sources);
	}
}
