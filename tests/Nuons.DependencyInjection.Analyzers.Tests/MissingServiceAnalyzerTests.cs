namespace Nuons.DependencyInjection.Analyzers.Tests;

public class MissingServiceAnalyzerTests(NuonsAnalyzerFixture fixture) : IClassFixture<NuonsAnalyzerFixture>
{
	[Fact]
	public async Task MissingService_WithInjected_ReportsDiagnostic()
	{
		const string testCode = @"
using Nuons.DependencyInjection;

internal class [|TestClass|] 
{
	[Injected]
	private readonly TestClass field;
}";

		await fixture.VerifyAnalyzerAsync<MissingServiceAnalyzer>(testCode);
	}

	[Fact]
	public async Task MissingService_WithInjectedOptions_ReportsDiagnostic()
	{
		const string testCode = @"
using Nuons.DependencyInjection;

internal class [|TestClass|] 
{
	[InjectedOptions]
	private readonly TestClass field;
}";

		await fixture.VerifyAnalyzerAsync<MissingServiceAnalyzer>(testCode);
	}

	[Theory]
	[InlineData("Transient(typeof(TestClass))")]
	[InlineData("Scoped(typeof(TestClass))")]
	[InlineData("Singleton(typeof(TestClass))")]
	[InlineData("Service(Lifetime.Transient, typeof(TestClass))")]
	[InlineData("InjectedConstructor")]
	public async Task AttributePresent_NoDiagnostic(string attribute)
	{
		string testCode = $@"
using Nuons.DependencyInjection;

[{attribute}]
internal class TestClass
{{
	[Injected]
	private readonly TestClass field;
}}";

		await fixture.VerifyAnalyzerAsync<MissingServiceAnalyzer>(testCode);
	}

	[Fact]
	public async Task ClassWithoutInjectedField_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.DependencyInjection;

internal class TestClass
{
	private readonly string field;
}";

		await fixture.VerifyAnalyzerAsync<MissingServiceAnalyzer>(testCode);
	}
}
