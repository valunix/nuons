using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Registration;

namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class ServiceRegistrationGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<ServiceRegistrationGenerator>(output);

	[Fact]
	public Task ServiceRegistrationsAreGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<ServiceRegistrationGenerator>();
		return Verify(sources);
	}
}
