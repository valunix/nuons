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
		Compilation? referenceCompilation = null;
		if (testContext.ReferencesSourcePath is not null)
		{
			var referencesSource = File.ReadAllText(testContext.ReferencesSourcePath);
			referenceCompilation = CreateCompilation(referencesSource, testContext.AssemblyMarkers);
		}

		var inputSource = File.ReadAllText(testContext.InputSourcePath);
		var compilation = CreateCompilation(inputSource, testContext.AssemblyMarkers, referenceCompilation);

		var generator = new TGenerator();
		var driver = CSharpGeneratorDriver.Create(generator)
			.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
		return driver;
	}

	private Compilation CreateCompilation(string targetSource, Type[] assemblyMarkers, Compilation? referenceCompilation = null)
	{
		var references = assemblyMarkers
			.Select(type => type.Assembly.Location)
			.Select(location => MetadataReference.CreateFromFile(location) as MetadataReference)
			.ToList();

		references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

		var runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();

		var systemRuntimePath = Path.Combine(runtimeDir, "System.Runtime.dll");
		references.Add(MetadataReference.CreateFromFile(systemRuntimePath));

		var netStandardPath = Path.Combine(runtimeDir, "netstandard.dll");
		references.Add(MetadataReference.CreateFromFile(netStandardPath));

		if (referenceCompilation is not null)
		{
			using var peStream = new MemoryStream();
			var emit = referenceCompilation.Emit(peStream);
			if (!emit.Success)
			{
				throw new InvalidOperationException("Unable to emit reference compilation.");
			}
			peStream.Position = 0;
			references.Add(MetadataReference.CreateFromStream(peStream));
		}

		var compilation = CSharpCompilation.Create(
			$"{GetType().Name}.TestAssembly",
			[CSharpSyntaxTree.ParseText(SourceText.From(targetSource, Encoding.UTF8))],
			[.. references],
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
