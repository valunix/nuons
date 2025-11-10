using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Registration;

namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class RootRegistrationGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact]
	public void DevRunGenerator() => fixture.RunGenerator<RootRegistrationGenerator>(output);

	[Fact]
	public Task RootRegistrationIsGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<RootRegistrationGenerator>();
		return Verify(sources);
	}
}
