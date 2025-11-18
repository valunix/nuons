namespace Nuons.DependencyInjection.Analyzers.Tests;

public class MissingServiceAnalyzerTests(NuonsAnalyzerFixture fixture) : IClassFixture<NuonsAnalyzerFixture>
{
	[Fact]
	public async Task MissingService_WithInjected_ReportsDiagnostic()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

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
using Nuons.DependencyInjection.Abstractions;

internal class [|TestClass|] 
{
	[InjectedOptions]
	private readonly TestClass field;
}";

		await fixture.VerifyAnalyzerAsync<MissingServiceAnalyzer>(testCode);
	}

	[Theory]
	[InlineData("Transient")]
	[InlineData("Scoped")]
	[InlineData("Singleton")]
	[InlineData("Service(Lifetime.Transient, typeof(TestClass))")]
	[InlineData("InjectedConstructor")]
	public async Task AttributePresent_NoDiagnostic(string attribute)
	{
		string testCode = $@"
using Nuons.DependencyInjection.Abstractions;

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
using Nuons.DependencyInjection.Abstractions;

internal class TestClass
{
	private readonly string field;
}";

		await fixture.VerifyAnalyzerAsync<MissingServiceAnalyzer>(testCode);
	}
}
