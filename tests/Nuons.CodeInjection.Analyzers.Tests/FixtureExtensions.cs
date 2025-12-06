using Microsoft.CodeAnalysis.Diagnostics;
using Nuons.Core.Tests;
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Analyzers.Tests;

public static class FixtureExtensions
{
	public static async Task VerifyAnalyzerAsync<TAnalyzer>(this NuonAnalyzerFixture fixture, string source)
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
		var context = new NuonAnalyzerTestContext(typeof(CodeInjectionAbstractionsAssemblyMarker));
		await fixture.VerifyAnalyzerAsync<TAnalyzer>(source, context, TestContext.Current.CancellationToken);
	}
}
