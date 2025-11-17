using Nuons.Core.Tests;
using Nuons.Http.Generators;

namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class EndpointGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact]
	public void DevRunGenerator() => fixture.RunGenerator<EndpointGenerator>(output);

	[Fact]
	public Task EndpointExtensionsAreGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<EndpointGenerator>();
		return Verify(sources);
	}
}
