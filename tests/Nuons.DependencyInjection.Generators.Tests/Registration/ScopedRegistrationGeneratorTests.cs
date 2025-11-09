using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Registration;

namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class ScopedRegistrationGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<ScopedRegistrationGenerator>(output);

	[Fact]
	public Task ScopedRegistrationsAreGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<ScopedRegistrationGenerator>();
		return Verify(sources);
	}
}
