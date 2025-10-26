namespace Nuons.DependencyInjection.Generators.Tests.Configuration;

public class OptionsRegistrationGeneratorTests : GeneratorTests, IClassFixture<OptionsRegistrationGeneratorFixture>
{
	private readonly ITestOutputHelper output;
	private readonly OptionsRegistrationGeneratorFixture fixture;

	public OptionsRegistrationGeneratorTests(ITestOutputHelper output, OptionsRegistrationGeneratorFixture fixture)
	{
		this.output = output;
		this.fixture = fixture;
	}

	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() =>
		RunGenerator(output, fixture);

	[Fact]
	public Task OptionsRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
