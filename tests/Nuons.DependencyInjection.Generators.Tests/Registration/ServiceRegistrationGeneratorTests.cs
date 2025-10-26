namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class ServiceRegistrationGeneratorTests(ITestOutputHelper output, ServiceRegistrationGeneratorFixture fixture)
	: GeneratorTests, IClassFixture<ServiceRegistrationGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => RunGenerator(output, fixture);

	[Fact]
	public Task ServiceRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
