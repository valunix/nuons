using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Startup;

namespace Nuons.DependencyInjection.Generators.Tests;

public class CombinedServiceRegistrationGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<CombinedServiceRegistrationGenerator>(output);

	[Fact]
	public Task ServiceRegistrationsAreGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<CombinedServiceRegistrationGenerator>();
		return Verify(sources);
	}
}
