namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class SingletonInjectionGeneratorTests : GeneratorTests, IClassFixture<SingletonInjectionGeneratorFixture>
{
	private readonly ITestOutputHelper output;
	private readonly SingletonInjectionGeneratorFixture fixture;

	public SingletonInjectionGeneratorTests(ITestOutputHelper output, SingletonInjectionGeneratorFixture fixture)
	{
		this.output = output;
		this.fixture = fixture;
	}

	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() =>
		RunGenerator(output, fixture);

	[Fact]
	public Task SingletonInjectionIsGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
