namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class RootRegistrationGeneratorTests : GeneratorTests, IClassFixture<RootRegistrationGeneratorFixture>
{
	private readonly ITestOutputHelper output;
	private readonly RootRegistrationGeneratorFixture fixture;

	public RootRegistrationGeneratorTests(ITestOutputHelper output, RootRegistrationGeneratorFixture fixture)
	{
		this.output = output;
		this.fixture = fixture;
	}

	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() =>
		RunGenerator(output, fixture);

	[Fact]
	public Task RootRegistrationIsGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
