namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class TransientRegistrationGeneratorTests(ITestOutputHelper output, TransientRegistrationGeneratorFixture fixture)
	: GeneratorTests, IClassFixture<TransientRegistrationGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => RunGenerator(output, fixture);

	[Fact]
	public Task TransientRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
