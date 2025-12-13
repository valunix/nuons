using Nuons.Core.Tests;

namespace Nuons.DependencyInjection.Generators.Tests;

public class ServiceRegistrationGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact]
	public void DevRunGenerator() => fixture.RunGenerator<ServiceRegistrationGenerator>(output);

	[Fact]
	public Task ServiceRegistrationsAreGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<ServiceRegistrationGenerator>();
		return Verify(sources);
	}
}
