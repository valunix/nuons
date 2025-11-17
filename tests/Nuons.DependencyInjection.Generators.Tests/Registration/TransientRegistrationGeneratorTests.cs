using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Registration;

namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class TransientRegistrationGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<TransientRegistrationGenerator>(output);

	[Fact]
	public Task TransientRegistrationsAreGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<TransientRegistrationGenerator>();
		return Verify(sources);
	}
}
