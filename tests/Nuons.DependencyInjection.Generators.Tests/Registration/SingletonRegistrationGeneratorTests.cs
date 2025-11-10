using Nuons.Core.Tests;
using Nuons.DependencyInjection.Generators.Registration;

namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class SingletonRegistrationGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<SingletonRegistrationGenerator>(output);

	[Fact]
	public Task SingletonRegistrationsAreGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<SingletonRegistrationGenerator>();
		return Verify(sources);
	}
}
