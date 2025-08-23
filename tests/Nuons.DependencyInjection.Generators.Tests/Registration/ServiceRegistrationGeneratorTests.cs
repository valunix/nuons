namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class ServiceRegistrationGeneratorTests : GeneratorTests, IClassFixture<ServiceRegistrationGeneratorFixture>
{
	private readonly ITestOutputHelper output;
	private readonly ServiceRegistrationGeneratorFixture fixture;

	public ServiceRegistrationGeneratorTests(ITestOutputHelper output, ServiceRegistrationGeneratorFixture fixture)
	{
		this.output = output;
		this.fixture = fixture;
	}

	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() =>
		RunGenerator(output, fixture);

	[Fact]
	public Task ServiceRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
