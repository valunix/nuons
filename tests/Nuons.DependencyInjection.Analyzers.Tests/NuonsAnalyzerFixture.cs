using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Nuons.DependencyInjection.Analyzers.Tests;

public class NuonsAnalyzerFixture
{
    private static readonly Assembly NuonsAssembly = typeof(AssemblyMarker).Assembly;

    public async Task VerifyAnalyzerAsync<TAnalyzer>(string source) 
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source
        };
        
        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(NuonsAssembly.Location));
        
        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
