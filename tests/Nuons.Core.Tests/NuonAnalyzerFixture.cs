using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Nuons.Core.Tests;

public class NuonAnalyzerFixture
{
	public async Task VerifyAnalyzerAsync<TAnalyzer>(string source, NuonAnalyzerTestContext context, CancellationToken cancellationToken)
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
		var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
		{
			TestCode = source
		};

		test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(context.AssemblyMarker.Assembly.Location));

		await test.RunAsync(cancellationToken);
	}
}
