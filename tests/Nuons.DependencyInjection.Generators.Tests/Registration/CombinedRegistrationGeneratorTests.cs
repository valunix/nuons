namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class CombinedRegistrationGeneratorTests(ITestOutputHelper output, CombinedRegistrationGeneratorFixture fixture)
	: GeneratorTests, IClassFixture<CombinedRegistrationGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => RunGenerator(output, fixture);

	[Fact]
	public Task CombinedRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
