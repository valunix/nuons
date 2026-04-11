using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Options;
using Nuons.Core.Abstractions;
using Nuons.Core.Tests;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators.Tests;

public class ServiceRegistrationGeneratorCachingTests(NuonGeneratorFixture fixture, ITestOutputHelper output)
	: IClassFixture<NuonGeneratorFixture>
{
	private static readonly Type[] AssemblyMarkers =
		[typeof(DIAbstractionsAssemblyMarker), typeof(Options), typeof(CoreAbstractionsAssemblyMarker)];

	private (CSharpGeneratorDriver Driver, Compilation Compilation) CreateDriverAndRun(string source)
	{
		var compilation = fixture.CreateCompilation(source, AssemblyMarkers);
		var generator = new ServiceRegistrationGenerator();
		var driver = CSharpGeneratorDriver.Create(
			generators: [generator.AsSourceGenerator()],
			driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)
		);

		driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
		return (driver, compilation);
	}

	private CSharpGeneratorDriver RunAgain(CSharpGeneratorDriver driver, string updatedSource)
	{
		var compilation = fixture.CreateCompilation(updatedSource, AssemblyMarkers);
		return (CSharpGeneratorDriver)driver.RunGenerators(compilation);
	}

	private static void AssertStepReason(GeneratorRunResult result, string stepName, IncrementalStepRunReason expectedReason)
	{
		result.TrackedSteps.ShouldContainKey(stepName);
		var steps = result.TrackedSteps[stepName];
		foreach (var step in steps)
		{
			foreach (var stepOutput in step.Outputs)
			{
				//stepOutput.Reason.ShouldBe(expectedReason, $"Step '{stepName}' expected {expectedReason} but was {stepOutput.Reason}");
				stepOutput.Reason.ShouldBe(expectedReason);
			}
		}
	}

	private static void LogTrackedSteps(GeneratorRunResult result, ITestOutputHelper output)
	{
		foreach (var (name, steps) in result.TrackedSteps)
		{
			foreach (var step in steps)
			{
				foreach (var stepOutput in step.Outputs)
				{
					output.WriteLine($"{name}: {stepOutput.Reason} (value: {stepOutput.Value})");
				}
			}
		}
	}

	[Fact]
	public void AllSteps_AreCached_WhenIdenticalSourceIsRerun()
	{
		const string source = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;
			[Singleton]
			public class MySingleton : IMySingleton { }
			public interface IMySingleton { }
			[Transient]
			public class MyTransient : IMyTransient { }
			public interface IMyTransient { }
			[Scoped]
			public class MyScoped : IMyScoped { }
			public interface IMyScoped { }
			""";

		var (driver, _) = CreateDriverAndRun(source);
		driver = RunAgain(driver, source);

		var result = driver.GetRunResult().Results[0];
		LogTrackedSteps(result, output);

		AssertStepReason(result, TrackingNames.SingletonProvider, IncrementalStepRunReason.Cached);
		AssertStepReason(result, TrackingNames.ScopedProvider, IncrementalStepRunReason.Cached);
		AssertStepReason(result, TrackingNames.TransientProvider, IncrementalStepRunReason.Cached);
		AssertStepReason(result, TrackingNames.AllRegistrations, IncrementalStepRunReason.Cached);
		AssertStepReason(result, TrackingNames.CombinedProvider, IncrementalStepRunReason.Cached);
	}

	[Fact]
	public void AllSteps_AreCached_WhenUnrelatedChangesAreMade()
	{
		const string source = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;

			[Singleton]
			public class MySingleton : IMySingleton { }
			public interface IMySingleton;

			[Transient]
			public class MyTransient : IMyTransient { }
			public interface IMyTransient;

			[Scoped]
			public class MyScoped : IMyScoped { }
			public interface IMyScoped;
			""";

		var (driver, _) = CreateDriverAndRun(source);

		const string updatedSource = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;

			[Singleton]
			public class MySingleton : IMySingleton { private int myField; }
			public interface IMySingleton;

			[Transient]
			public class MyTransient : IMyTransient { private int myField; }
			public interface IMyTransient;

			[Scoped]
			public class MyScoped : IMyScoped { private int myField; }
			public interface IMyScoped;

			public class UnrelatedClass;
			""";


		driver = RunAgain(driver, updatedSource);

		var result = driver.GetRunResult().Results[0];
		LogTrackedSteps(result, output);

		AssertStepReason(result, TrackingNames.SingletonProvider, IncrementalStepRunReason.Cached);
		AssertStepReason(result, TrackingNames.ScopedProvider, IncrementalStepRunReason.Cached);
		AssertStepReason(result, TrackingNames.TransientProvider, IncrementalStepRunReason.Cached);
		AssertStepReason(result, TrackingNames.AllRegistrations, IncrementalStepRunReason.Cached);
		AssertStepReason(result, TrackingNames.CombinedProvider, IncrementalStepRunReason.Cached);
	}
}
