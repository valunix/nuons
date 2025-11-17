using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Injection;

namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class InjectedConstructorGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<InjectedConstructorGenerator>(output);

	[Fact]
	public Task InjectedConstructorIsGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<InjectedConstructorGenerator>();
		return Verify(sources);
	}
}
