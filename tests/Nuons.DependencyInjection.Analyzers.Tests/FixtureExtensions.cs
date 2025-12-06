using Microsoft.CodeAnalysis.Diagnostics;
using Nuons.Core.Tests;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Analyzers.Tests;

public static class FixtureExtensions
{
	public static async Task VerifyAnalyzerAsync<TAnalyzer>(this NuonAnalyzerFixture fixture, string source)
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
		var context = new NuonAnalyzerTestContext(typeof(DIAbstractionsAssemblyMarker));
		await fixture.VerifyAnalyzerAsync<TAnalyzer>(source, context, TestContext.Current.CancellationToken);
	}
}
