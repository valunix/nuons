namespace Nuons.DependencyInjection.Generators.Tests.Configuration;

public class OptionsRegistrationGeneratorTests(ITestOutputHelper output, OptionsRegistrationGeneratorFixture fixture)
	: GeneratorTests, IClassFixture<OptionsRegistrationGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => RunGenerator(output, fixture);

	[Fact]
	public Task OptionsRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
