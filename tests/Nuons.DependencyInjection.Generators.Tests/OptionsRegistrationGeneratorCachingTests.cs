using Microsoft.CodeAnalysis;
using Nuons.Core.Tests;

namespace Nuons.DependencyInjection.Generators.Tests;

public class OptionsRegistrationGeneratorCachingTests(NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	private static readonly string[] AllTrackedSteps =
	[
		TrackingNames.OptionsProvider,
		TrackingNames.OptionsIncrementProvider,
	];

	[Fact]
	public void AllSteps_AreCached_WhenIdenticalSourceIsRerun()
	{
		const string source = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			[Options(nameof(MyOptions))]
			public class MyOptions { public string? Value { get; set; } }
			""";

		fixture.RunIncrementalGenerator<OptionsRegistrationGenerator>(source, source)
			.AssertAllStepsHaveReason(AllTrackedSteps, IncrementalStepRunReason.Cached);
	}

	[Fact]
	public void AllSteps_AreCached_WhenUnrelatedChangesAreMade()
	{
		const string source = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			[Options(nameof(MyOptions))]
			public class MyOptions { public string? Value { get; set; } }
			""";

		const string updatedSource = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			[Options(nameof(MyOptions))]
			public class MyOptions { public string? Value { get; set; } private int myField; }

			public class UnrelatedClass;
			""";

		fixture.RunIncrementalGenerator<OptionsRegistrationGenerator>(source, updatedSource)
			.AssertAllStepsHaveReason(AllTrackedSteps, IncrementalStepRunReason.Cached);
	}

	[Fact]
	public void AllPipelines_AreInvalidated_WhenNewOptionsAreAdded()
	{
		const string source = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			[Options(nameof(MyOptions))]
			public class MyOptions { public string? Value { get; set; } }
			""";

		const string updatedSource = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			[Options(nameof(MyOptions))]
			public class MyOptions { public string? Value { get; set; } }

			[Options(nameof(MyOtherOptions))]
			public class MyOtherOptions { public int Count { get; set; } }
			""";

		fixture.RunIncrementalGenerator<OptionsRegistrationGenerator>(source, updatedSource)
			.AssertAllStepsHaveReason(AllTrackedSteps, IncrementalStepRunReason.Modified);
	}

	[Fact]
	public void AllPipelines_AreInvalidated_WhenValidateFlagChanges()
	{
		const string source = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			[Options(nameof(MyOptions))]
			public class MyOptions { public string? Value { get; set; } }
			""";

		const string updatedSource = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			[Options(nameof(MyOptions), Validate = true)]
			public class MyOptions { public string? Value { get; set; } }
			""";

		fixture.RunIncrementalGenerator<OptionsRegistrationGenerator>(source, updatedSource)
			.AssertAllStepsHaveReason(AllTrackedSteps, IncrementalStepRunReason.Modified);
	}

	[Fact]
	public void AllPipelines_AreInvalidated_WhenValidateOnStartFlagChanges()
	{
		const string source = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			[Options(nameof(MyOptions))]
			public class MyOptions { public string? Value { get; set; } }
			""";

		const string updatedSource = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			[Options(nameof(MyOptions), ValidateOnStart = true)]
			public class MyOptions { public string? Value { get; set; } }
			""";

		fixture.RunIncrementalGenerator<OptionsRegistrationGenerator>(source, updatedSource)
			.AssertAllStepsHaveReason(AllTrackedSteps, IncrementalStepRunReason.Modified);
	}
}
