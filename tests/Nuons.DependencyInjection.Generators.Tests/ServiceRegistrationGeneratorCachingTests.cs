using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Options;
using Nuons.Core.Abstractions;
using Nuons.Core.Tests;
using Nuons.DependencyInjection.Abstractions;
using Shouldly;
using Xunit;

namespace Nuons.DependencyInjection.Generators.Tests;

public class ServiceRegistrationGeneratorCachingTests(NuonGeneratorFixture fixture, ITestOutputHelper output2)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact]
	public void AssemblyName_ShouldBeCached_WhenUnrelatedChangeOccurs()
	{
		// 1. Setup initial compilation
		var initialSource = @"
using Nuons.DependencyInjection.Abstractions;
namespace Test;
[Singleton]
public class MyService { }
";
		var assemblyMarkers = new[] { typeof(DIAbstractionsAssemblyMarker), typeof(Options), typeof(CoreAbstractionsAssemblyMarker) };

		var compilation = fixture.CreateCompilation(initialSource, assemblyMarkers);

		//compilation = compilation.WithAssemblyName("TestAssembly");

		var generator = new ServiceRegistrationGenerator();
		var driver = CSharpGeneratorDriver.Create(
			generators: [generator.AsSourceGenerator()],
			driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true)
		);

		// 2. Initial Run
		driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);

		// 3. Modified Run (Unrelated change: adding a comment)

		var updatedSource = @"
using Nuons.DependencyInjection.Abstractions;
namespace Test;
[Singleton]
public class MyService : MyServiceB { private string pero; }
public class MyServiceB { }
";

		//var compilation = fixture.CreateCompilation(initialSource, assemblyMarkers);
		//var modifiedCompilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("// Just a comment"));
		var modifiedCompilation = fixture.CreateCompilation(updatedSource, assemblyMarkers);

		driver = (CSharpGeneratorDriver)driver.RunGenerators(modifiedCompilation);

		// 4. Verify Caching
		var runResult = driver.GetRunResult();
		var result = runResult.Results[0];


		result.TrackedSteps.ShouldNotBeEmpty();

		result.TrackedSteps.ShouldContainKey("AssemblyName");

		var assemblyNameSteps = result.TrackedSteps["AssemblyName"];
		foreach (var step in assemblyNameSteps)
		{
			foreach (var output in step.Outputs)
			{
				output.Reason.ShouldBe(IncrementalStepRunReason.Unchanged);
			}
		}

		result.TrackedSteps.ShouldContainKey("samac");
		var singeltonSteps = result.TrackedSteps["samac"];
		foreach (var step in singeltonSteps)
		{
			foreach (var output in step.Outputs)
			{
				//output.Reason.ShouldBe(IncrementalStepRunReason.Unchanged);
				output2.WriteLine($"samac: {output.Reason}");
			}
		}
	}
}
