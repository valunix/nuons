namespace Nuons.DependencyInjection.Analyzers.Tests;

public class MissingServiceAnalyzerTests(NuonsAnalyzerFixture fixture) : IClassFixture<NuonsAnalyzerFixture>
{
	[Fact]
	public async Task MissingService_ReportsWarning()
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
	public async Task AttributePresent_NoWarning()
	{
		const string testCode = @"
using Nuons.DependencyInjection;

[Scoped(typeof(TestClass))]
internal class TestClass
{
	[Injected]
	private readonly TestClass field;
}";

		await fixture.VerifyAnalyzerAsync<MissingServiceAnalyzer>(testCode);
	}
	
	[Fact]
	public async Task ClassWithoutInjectedField_NoWarning()
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
