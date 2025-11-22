using Nuons.Core.Tests;

namespace Nuons.Http.Generators.Tests;

public class EndpointGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => fixture.RunGenerator<EndpointGenerator>(output);

	[Fact]
	public Task EndpointExtensionsAreGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<EndpointGenerator>();
		return Verify(sources);
	}
}
