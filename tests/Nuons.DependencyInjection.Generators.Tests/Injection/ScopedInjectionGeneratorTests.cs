namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class ScopedInjectionGeneratorTests : GeneratorTests, IClassFixture<ScopedInjectionGeneratorFixture>
{
	private readonly ITestOutputHelper output;
	private readonly ScopedInjectionGeneratorFixture fixture;

	public ScopedInjectionGeneratorTests(ITestOutputHelper output, ScopedInjectionGeneratorFixture fixture)
	{
		this.output = output;
		this.fixture = fixture;
	}

	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() =>
		RunGenerator(output, fixture);

	[Fact]
	public Task ScopedInjectionIsGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
