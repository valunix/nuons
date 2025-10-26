namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class RootRegistrationGeneratorTests(ITestOutputHelper output, RootRegistrationGeneratorFixture fixture)
	: GeneratorTests, IClassFixture<RootRegistrationGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => RunGenerator(output, fixture);

	[Fact]
	public Task RootRegistrationIsGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
