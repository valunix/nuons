using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Nuons.Core.Tests;

public class NuonGeneratorFixture : IDisposable
{
	public string GenerateSources<TGenerator>(NuonTestContext testContext)
		where TGenerator : IIncrementalGenerator, new()
	{
		var driver = Drive<TGenerator>(testContext);

		var generatedSources = driver.GetRunResult().Results
			.SelectMany(result => result.GeneratedSources)
			.Select(source => source.SourceText.ToString())
			.ToList();

		if (generatedSources.Count != 0)
		{
			return generatedSources.Aggregate((source1, source2) => $"{source1}{"\n"}{source2}");
		}
		else
		{
			return string.Empty;
		}
	}

	private GeneratorDriver Drive<TGenerator>(NuonTestContext testContext)
		where TGenerator : IIncrementalGenerator, new()
	{
		var inputSource = File.ReadAllText(testContext.InputSourcePath);
		var generator = new TGenerator();
		var compilation = CreateCompilation(inputSource, testContext.AssemblyMarkers);
		var driver = CSharpGeneratorDriver.Create(generator)
			.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
		return driver;
	}

	private Compilation CreateCompilation(string targetSource, Type[] assemblyMarkers)
	{
		var references = assemblyMarkers
			.Select(type => type.Assembly.Location)
			.Select(location => MetadataReference.CreateFromFile(location))
			.ToArray();

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
				..references
			],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
		);

		return compilation;
	}

	public void RunGenerator<TGenerator>(NuonTestContext testContext, ITestOutputHelper output)
		where TGenerator : IIncrementalGenerator, new()
	{
		var driver = Drive<TGenerator>(testContext);
		var runResult = driver.GetRunResult();

		LogResults(runResult, output);
	}

	private void LogResults(GeneratorDriverRunResult runResult, ITestOutputHelper output)
	{
		output.WriteLine("------------------------------------ START errors ------------------------------------");
		runResult.Results
			.Select(result => result.Exception)
			.OfType<Exception>()
			.Select(e => e.Message)
			.ToList()
			.ForEach(message =>
			{
				output.WriteLine(message);
				output.WriteLine(string.Empty);
			});
		output.WriteLine("------------------------------------ END errors ------------------------------------");

		output.WriteLine(string.Empty);

		output.WriteLine("------------------------------------ START sources ------------------------------------");
		runResult.Results
			.SelectMany(result => result.GeneratedSources)
			.Select(source => source.SourceText)
			.ToList()
			.ForEach(sourceText =>
			{
				output.WriteLine(sourceText.ToString());
				output.WriteLine(string.Empty);
			});
		output.WriteLine("------------------------------------ END sources ------------------------------------");
	}

	public void Dispose()
	{
		// do nothing
	}
}
