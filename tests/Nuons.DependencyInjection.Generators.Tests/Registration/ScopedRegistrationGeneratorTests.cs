namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class ScopedRegistrationGeneratorTests(ITestOutputHelper output, ScopedRegistrationGeneratorFixture fixture)
	: GeneratorTests, IClassFixture<ScopedRegistrationGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => RunGenerator(output, fixture);

	[Fact]
	public Task ScopedRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
