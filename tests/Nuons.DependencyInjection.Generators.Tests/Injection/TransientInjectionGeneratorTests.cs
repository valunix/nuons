using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Injection;

namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class TransientInjectionGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<TransientInjectionGenerator>(output);

	[Fact]
	public Task TransientInjectionIsGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<TransientInjectionGenerator>();
		return Verify(sources);
	}
}
