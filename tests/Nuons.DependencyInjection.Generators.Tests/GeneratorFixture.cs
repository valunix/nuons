using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Options;

namespace Nuons.DependencyInjection.Generators.Tests;

public abstract class GeneratorFixture : IDisposable
{
	public GeneratorDriver Driver { get; private init; }

	public GeneratorFixture()
	{
		var generator = NewGenerator();
		Driver = CSharpGeneratorDriver.Create(generator);
	}

	protected abstract IIncrementalGenerator NewGenerator();

	public GeneratorDriver Drive(string source)
	{
		var compilation = CreateCompilationWithReference(source);
		var driver = Driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
		return driver;
	}

	private Compilation CreateCompilationWithReference(string targetSource)
	{
		var attributeAssemblyPath = typeof(AssemblyMarker).Assembly.Location;

		var runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
		var systemRuntimePath = Path.Combine(runtimeDir, "System.Runtime.dll");
		var netStandardPath = Path.Combine(runtimeDir, "netstandard.dll");

		var compilation = CSharpCompilation.Create(
			$"{GetType().Name}.TestAssembly",
			[CSharpSyntaxTree.ParseText(SourceText.From(targetSource, Encoding.UTF8))],
			[
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(netStandardPath),
				MetadataReference.CreateFromFile(systemRuntimePath),
				MetadataReference.CreateFromFile(typeof(Options).Assembly.Location),
				MetadataReference.CreateFromFile(attributeAssemblyPath),
			],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
		);

		return compilation;
	}

	public void Dispose()
	{
		// do nothing
	}
}
