namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class TransientRegistrationGeneratorTests : GeneratorTests, IClassFixture<TransientRegistrationGeneratorFixture>
{
	private readonly ITestOutputHelper output;
	private readonly TransientRegistrationGeneratorFixture fixture;

	public TransientRegistrationGeneratorTests(ITestOutputHelper output, TransientRegistrationGeneratorFixture fixture)
	{
		this.output = output;
		this.fixture = fixture;
	}

	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() =>
		RunGenerator(output, fixture);

	[Fact]
	public Task TransientRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
