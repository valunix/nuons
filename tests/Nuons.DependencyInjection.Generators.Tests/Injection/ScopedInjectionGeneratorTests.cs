using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Injection;

namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class ScopedInjectionGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<ScopedInjectionGenerator>(output);

	[Fact]
	public Task ScopedInjectionIsGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<ScopedInjectionGenerator>();
		return Verify(sources);
	}
}
