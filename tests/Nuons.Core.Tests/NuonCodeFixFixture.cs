using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Nuons.Core.Tests;

public class NuonCodeFixFixture
{
	public async Task VerifyCodeFixAsync<TAnalyzer, TCodeFix>(string source, string fixedSource, NuonAnalyzerTestContext context, CancellationToken cancellationToken)
		where TAnalyzer : DiagnosticAnalyzer, new()
		where TCodeFix : CodeFixProvider, new()
	{
		var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
		{
			TestCode = source,
			FixedCode = fixedSource,
		};

		test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(context.AssemblyMarker.Assembly.Location));
		test.FixedState.AdditionalReferences.Add(MetadataReference.CreateFromFile(context.AssemblyMarker.Assembly.Location));

		await test.RunAsync(cancellationToken);
	}
}
