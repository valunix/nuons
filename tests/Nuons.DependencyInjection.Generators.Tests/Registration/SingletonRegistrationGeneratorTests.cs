namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class SingletonRegistrationGeneratorTests(ITestOutputHelper output, SingletonRegistrationGeneratorFixture fixture)
	: GeneratorTests, IClassFixture<SingletonRegistrationGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => RunGenerator(output, fixture);

	[Fact]
	public Task SingletonRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
