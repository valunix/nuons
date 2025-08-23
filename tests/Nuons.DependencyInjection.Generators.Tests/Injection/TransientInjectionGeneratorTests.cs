namespace Nuons.DependencyInjection.Generators.Tests.Injection;

public class TransientInjectionGeneratorTests : GeneratorTests, IClassFixture<TransientInjectionGeneratorFixture>
{
	private readonly ITestOutputHelper output;
	private readonly TransientInjectionGeneratorFixture fixture;

	public TransientInjectionGeneratorTests(ITestOutputHelper output, TransientInjectionGeneratorFixture fixture)
	{
		this.output = output;
		this.fixture = fixture;
	}

	[Fact(Skip = "For debugging during dev only")]
	public void DevRunGenerator() =>
		RunGenerator(output, fixture);

	[Fact]
	public Task TransientInjectionIsGeneratedCorrectly()
	{
		var sources = GenerateSources(fixture);
		return Verify(sources);
	}
}
