using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Analyzers.Tests;

public class MissingServiceAnalyzerTests(NuonAnalyzerFixture fixture) : IClassFixture<NuonAnalyzerFixture>
{
	[Fact]
	public async Task MissingService_WithInjected_ReportsDiagnostic()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

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
using Nuons.CodeInjection.Abstractions;

internal class [|TestClass|] 
{
	[InjectedOptions]
	private readonly TestClass field;
}";

		await fixture.VerifyAnalyzerAsync<MissingServiceAnalyzer>(testCode);
	}

	[Fact]
	public async Task AttributePresent_NoDiagnostic()
	{
		string testCode = $@"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
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
using Nuons.CodeInjection.Abstractions;

internal class TestClass
{
	private readonly string field;
}";

		await fixture.VerifyAnalyzerAsync<MissingServiceAnalyzer>(testCode);
	}
}
