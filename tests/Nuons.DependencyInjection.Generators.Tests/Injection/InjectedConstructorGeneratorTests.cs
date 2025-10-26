namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class InjectedConstructorGeneratorTests(ITestOutputHelper output, InjectedConstructorGeneratorFixture fixture)
	: GeneratorTests, IClassFixture<InjectedConstructorGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => RunGenerator(output, fixture);

	[Fact]
	public Task InjectedConstructorIsGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
