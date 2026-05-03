using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Generators.Tests.Injection;

public class AssemblyMarkerGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact]
	public void DevRunGenerator() => fixture.RunGenerator<AssemblyMarkerGenerator>(output);

	[Fact]
	public Task AssemblyMarkerIsEmittedExactlyOnce()
	{
		var sources = fixture.GenerateSources<AssemblyMarkerGenerator>();
		return Verify(sources);
	}
}
