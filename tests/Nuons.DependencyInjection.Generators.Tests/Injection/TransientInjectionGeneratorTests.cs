namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class TransientInjectionGeneratorTests(ITestOutputHelper output, TransientInjectionGeneratorFixture fixture)
	: GeneratorTests, IClassFixture<TransientInjectionGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => RunGenerator(output, fixture);

	[Fact]
	public Task TransientInjectionIsGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
