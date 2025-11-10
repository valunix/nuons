using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Injection;

namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class ServiceInjectionGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<ServiceInjectionGenerator>(output);

	[Fact]
	public Task ServiceInjectionIsGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<ServiceInjectionGenerator>();
		return Verify(sources);
	}
}
