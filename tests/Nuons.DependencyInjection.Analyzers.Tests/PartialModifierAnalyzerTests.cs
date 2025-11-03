namespace Nuons.DependencyInjection.Analyzers.Tests;

public class PartialModifierAnalyzerTests(NuonsAnalyzerFixture fixture) : IClassFixture<NuonsAnalyzerFixture>
{
	[Fact]
	public async Task ClassWithServiceAttribute_MissingPartialModifier_ReportsError()
	{
		const string testCode = @"
using Nuons.DependencyInjection;

[Scoped]
public class [|TestClass|] {}";

		await fixture.VerifyAnalyzerAsync<PartialModifierAnalyzer>(testCode);
	}

	[Fact]
	public async Task ClassWithPartialModifier_NoError()
	{
		const string testCode = @"
using Nuons.DependencyInjection;

[Scoped]
public partial class TestClass {}";

		await fixture.VerifyAnalyzerAsync<PartialModifierAnalyzer>(testCode);
	}

	[Fact]
	public async Task ClassWithoutServiceAttribute_NoError()
	{
		const string testCode = @"
using Nuons.DependencyInjection;

public class TestClass {}";

		await fixture.VerifyAnalyzerAsync<PartialModifierAnalyzer>(testCode);
	}
}
