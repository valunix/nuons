using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Nuons.Core.Tests;

public class NuonGeneratorFixture : IDisposable
{
	public string GenerateSources<TGenerator>(NuonGeneratorTestContext testContext)
		where TGenerator : IIncrementalGenerator, new()
	{
		testContext.Increments.Length.ShouldBe(1);

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

	private GeneratorDriver Drive<TGenerator>(NuonGeneratorTestContext testContext, bool trackIncrementalSteps = false)
		where TGenerator : IIncrementalGenerator, new()
	{
		var generator = new TGenerator();
		GeneratorDriver driver = CSharpGeneratorDriver.Create(
			generators: [generator.AsSourceGenerator()],
			driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: trackIncrementalSteps)
		);

		foreach (var increment in testContext.Increments)
		{
			Compilation? referenceCompilation = null;
			if (increment.ReferencesSource is not null)
			{
				referenceCompilation = CreateCompilation(increment.ReferencesSource, testContext.AssemblyMarkers);
			}

			var compilation = CreateCompilation(increment.Source, testContext.AssemblyMarkers, referenceCompilation);
			driver = driver.RunGenerators(compilation);
		}

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

	public GeneratorRunResult RunIncrementalGenerator<TGenerator>(NuonGeneratorTestContext context)
		where TGenerator : IIncrementalGenerator, new()
	{
		var driver = Drive<TGenerator>(context, trackIncrementalSteps: true);
		return driver.GetRunResult().Results[0];
	}

	public void RunGenerator<TGenerator>(NuonGeneratorTestContext testContext, ITestOutputHelper output)
		where TGenerator : IIncrementalGenerator, new()
	{
		Drive<TGenerator>(testContext).GetRunResult().Log(output);
	}

	public void Dispose()
	{
		// do nothing
	}
}
