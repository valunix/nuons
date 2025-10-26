namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class ServiceInjectionGeneratorTests : GeneratorTests, IClassFixture<ServiceInjectionGeneratorFixture>
{
	private readonly ITestOutputHelper output;
	private readonly ServiceInjectionGeneratorFixture fixture;

	public ServiceInjectionGeneratorTests(ITestOutputHelper output, ServiceInjectionGeneratorFixture fixture)
	{
		this.output = output;
		this.fixture = fixture;
	}

	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() =>
		RunGenerator(output, fixture);

	[Fact]
	public Task ServiceInjectionIsGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
