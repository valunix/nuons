using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Nuons.CodeInjection.Abstractions;
using Nuons.Core.Tests;

namespace Nuons.CodeInjection.CodeFixes.Tests;

public static class FixtureExtensions
{
	public static async Task VerifyCodeFixAsync<TAnalyzer, TCodeFix>(this NuonCodeFixFixture fixture, string source, string fixedSource)
		where TAnalyzer : DiagnosticAnalyzer, new()
		where TCodeFix : CodeFixProvider, new()
	{
		var context = new NuonAnalyzerTestContext(typeof(CodeInjectionAbstractionsAssemblyMarker));
		await fixture.VerifyCodeFixAsync<TAnalyzer, TCodeFix>(source, fixedSource, context, TestContext.Current.CancellationToken);
	}
}
