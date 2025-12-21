using Nuons.Core.Tests;

namespace Nuons.DependencyInjection.Generators.Tests;

public class OptionsRegistrationGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<OptionsRegistrationGenerator>(output);

	[Fact]
	public Task OptionsRegistrationsAreGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<OptionsRegistrationGenerator>();
		return Verify(sources);
	}
}
