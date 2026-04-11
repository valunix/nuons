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
				stepOutput.Reason.ShouldBe(expectedReason, $"Step '{stepName}' expected {expectedReason} but was {stepOutput.Reason}");
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
	public void UnrelatedChange_ShouldCacheAssemblyNameAndRegistrations()
	{
		// Arrange - initial source with a singleton service
		const string initialSource = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;
			[Singleton]
			public class MyService : IMyService { }
			public interface IMyService { }
			""";

		var (driver, _) = CreateDriverAndRun(initialSource);

		// Act - add an unrelated class (no attributes)
		const string updatedSource = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;
			[Singleton]
			public class MyService : IMyService { }
			public interface IMyService { }
			public class UnrelatedClass { }
			""";

		driver = RunAgain(driver, updatedSource);

		// Assert
		var result = driver.GetRunResult().Results[0];
		LogTrackedSteps(result, output);

		AssertStepReason(result, TrackingNames.AssemblyName, IncrementalStepRunReason.Unchanged);
		AssertStepReason(result, TrackingNames.SingletonProvider, IncrementalStepRunReason.Cached);
	}

	[Fact]
	public void ModifiedRegistration_ShouldInvalidateRegistrations()
	{
		// Arrange - singleton service implementing one interface
		const string initialSource = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;
			[Singleton]
			public class MyService : IMyService { }
			public interface IMyService { }
			""";

		var (driver, _) = CreateDriverAndRun(initialSource);

		// Act - change the service to implement a different interface
		const string updatedSource = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;
			[Singleton]
			public class MyService : IOtherService { }
			public interface IMyService { }
			public interface IOtherService { }
			""";

		driver = RunAgain(driver, updatedSource);

		// Assert
		var result = driver.GetRunResult().Results[0];
		LogTrackedSteps(result, output);

		AssertStepReason(result, TrackingNames.AssemblyName, IncrementalStepRunReason.Unchanged);
		AssertStepReason(result, TrackingNames.SingletonProvider, IncrementalStepRunReason.Modified);
	}

	[Fact]
	public void IdenticalRerun_ShouldCacheEverything()
	{
		// Arrange & Act - run the exact same source twice
		const string source = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;
			[Singleton]
			public class MySingleton : IMySingleton { }
			public interface IMySingleton { }
			[Transient]
			public class MyTransient : IMyTransient { }
			public interface IMyTransient { }
			""";

		var (driver, _) = CreateDriverAndRun(source);
		driver = RunAgain(driver, source);

		// Assert - everything should be cached
		var result = driver.GetRunResult().Results[0];
		LogTrackedSteps(result, output);

		AssertStepReason(result, TrackingNames.AssemblyName, IncrementalStepRunReason.Unchanged);
		AssertStepReason(result, TrackingNames.SingletonProvider, IncrementalStepRunReason.Cached);
		AssertStepReason(result, TrackingNames.TransientProvider, IncrementalStepRunReason.Cached);
	}

	[Fact]
	public void CombinedProvider_ShouldBeCached_WhenAttributedClassBodyChanges()
	{
		// When the body of an attributed class changes (but not its type identity),
		// ForAttributeWithMetadataName re-evaluates and finds the same ServiceRegistration
		// (Unchanged). Collect sees all items Unchanged and stays Cached.
		// Combine(Unchanged, Cached) propagates Cached to CombinedProvider.
		const string source = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;
			[Singleton]
			public class ServiceA : IServiceA { }
			public interface IServiceA { }
			[Singleton<IServiceA>]
			public class ServiceB : IServiceA { }
			""";

		var compilation = fixture.CreateCompilation(source, AssemblyMarkers);
		var generator = new ServiceRegistrationGenerator();
		var driver = CSharpGeneratorDriver.Create(
			generators: [generator.AsSourceGenerator()],
			driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)
		);

		driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);

		// Add a field — syntax changes but extracted registration is identical
		var updatedSource = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;
			[Singleton]
			public class ServiceA : IServiceA { private int _x; }
			public interface IServiceA { }
			[Singleton<IServiceA>]
			public class ServiceB : IServiceA { }
			""";
		var modifiedCompilation = fixture.CreateCompilation(updatedSource, AssemblyMarkers);
		driver = (CSharpGeneratorDriver)driver.RunGenerators(modifiedCompilation);

		var result = driver.GetRunResult().Results[0];
		LogTrackedSteps(result, output);

		AssertStepReason(result, TrackingNames.AssemblyName, IncrementalStepRunReason.Unchanged);
		AssertStepReason(result, TrackingNames.CombinedProvider, IncrementalStepRunReason.Cached);
	}

	[Fact]
	public void NewRegistrationAdded_ShouldModifyAllRegistrations()
	{
		// Arrange - start with one singleton
		const string initialSource = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;
			[Singleton]
			public class MyService : IMyService { }
			public interface IMyService { }
			""";

		var (driver, _) = CreateDriverAndRun(initialSource);

		// Act - add a second singleton
		const string updatedSource = """
			using Nuons.DependencyInjection.Abstractions;
			namespace Test;
			[Singleton]
			public class MyService : IMyService { }
			public interface IMyService { }
			[Singleton]
			public class AnotherService : IAnotherService { }
			public interface IAnotherService { }
			""";

		driver = RunAgain(driver, updatedSource);

		// Assert
		var result = driver.GetRunResult().Results[0];
		LogTrackedSteps(result, output);

		AssertStepReason(result, TrackingNames.AssemblyName, IncrementalStepRunReason.Unchanged);
		AssertStepReason(result, TrackingNames.AllRegistrations, IncrementalStepRunReason.Modified);
	}
}
