namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class ScopedInjectionGeneratorTests(ITestOutputHelper output, ScopedInjectionGeneratorFixture fixture)
	: GeneratorTests, IClassFixture<ScopedInjectionGeneratorFixture>
{
	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() => RunGenerator(output, fixture);

	[Fact]
	public Task ScopedInjectionIsGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
