namespace Nuons.DependencyInjection.Generators.Tests.Registration;

public class SingletonRegistrationGeneratorTests : GeneratorTests, IClassFixture<SingletonRegistrationGeneratorFixture>
{
	private readonly ITestOutputHelper output;
	private readonly SingletonRegistrationGeneratorFixture fixture;

	public SingletonRegistrationGeneratorTests(ITestOutputHelper output, SingletonRegistrationGeneratorFixture fixture)
	{
		this.output = output;
		this.fixture = fixture;
	}

	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() =>
		RunGenerator(output, fixture);

	[Fact]
	public Task SingletonRegistrationsAreGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
