namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class ScopedRegistrationGeneratorTests : GeneratorTests, IClassFixture<ScopedRegistrationGeneratorFixture>
{
	private readonly ITestOutputHelper output;
	private readonly ScopedRegistrationGeneratorFixture fixture;

	public ScopedRegistrationGeneratorTests(ITestOutputHelper output, ScopedRegistrationGeneratorFixture fixture)
	{
		this.output = output;
		this.fixture = fixture;
	}

	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() =>
		RunGenerator(output, fixture);

	[Fact]
	public Task ScopedRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
