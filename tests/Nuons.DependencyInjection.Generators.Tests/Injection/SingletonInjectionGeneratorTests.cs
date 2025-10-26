namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class SingletonInjectionGeneratorTests(ITestOutputHelper output, SingletonInjectionGeneratorFixture fixture)
	: GeneratorTests, IClassFixture<SingletonInjectionGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => RunGenerator(output, fixture);

	[Fact]
	public Task SingletonInjectionIsGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
