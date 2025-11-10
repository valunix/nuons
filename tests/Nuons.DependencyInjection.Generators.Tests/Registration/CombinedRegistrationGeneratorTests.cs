using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Registration;

namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class CombinedRegistrationGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<CombinedRegistrationGenerator>(output);

	[Fact]
	public Task CombinedRegistrationsAreGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<CombinedRegistrationGenerator>();
		return Verify(sources);
	}
}
