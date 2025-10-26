namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class CombinedRegistrationGeneratorTests : GeneratorTests, IClassFixture<CombinedRegistrationGeneratorFixture>
{
	private readonly ITestOutputHelper output;
	private readonly CombinedRegistrationGeneratorFixture fixture;

	public CombinedRegistrationGeneratorTests(ITestOutputHelper output, CombinedRegistrationGeneratorFixture fixture)
	{
		this.output = output;
		this.fixture = fixture;
	}

	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() =>
		RunGenerator(output, fixture);

	[Fact]
	public Task CombinedRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
