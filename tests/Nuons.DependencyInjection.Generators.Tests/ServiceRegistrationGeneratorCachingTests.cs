using Microsoft.CodeAnalysis;
using Nuons.Core.Tests;

namespace Nuons.DependencyInjection.Generators.Tests;

public class ServiceRegistrationGeneratorCachingTests(NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	private static readonly string[] AllTrackedSteps =
	[
		TrackingNames.ServiceProviderNonGeneric(Lifetime.Singleton),
		TrackingNames.ServiceProviderNonGeneric(Lifetime.Scoped),
		TrackingNames.ServiceProviderNonGeneric(Lifetime.Transient),
		TrackingNames.ServiceProviderGeneric(Lifetime.Singleton),
		TrackingNames.ServiceProviderGeneric(Lifetime.Scoped),
		TrackingNames.ServiceProviderGeneric(Lifetime.Transient),
		TrackingNames.SingletonProvider,
		TrackingNames.ScopedProvider,
		TrackingNames.TransientProvider,
		TrackingNames.AllRegistrations,
		TrackingNames.IncrementProvider,
	];

	[Fact]
	public void AllSteps_AreCached_WhenIdenticalSourceIsRerun()
	{
		const string source = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			public interface IMySingleton;

			[Singleton]
			public class MySingleton : IMySingleton { }

			[Singleton<IMySingleton>]
			public class MySingletonGeneric : IMySingleton { }

			public interface IMyTransient;

			[Transient]
			public class MyTransient : IMyTransient { }

			[Transient<IMyTransient>]
			public class MyTransientGeneric : IMyTransient { }

			public interface IMyScoped;

			[Scoped]
			public class MyScoped : IMyScoped { }

			[Scoped<IMyScoped>]
			public class MyScopedGeneric : IMyScoped { }
			""";

		fixture.RunIncrementalGenerator<ServiceRegistrationGenerator>(source, source)
			.AssertAllStepsHaveReason(AllTrackedSteps, IncrementalStepRunReason.Cached);
	}

	[Fact]
	public void AllSteps_AreCached_WhenUnrelatedChangesAreMade()
	{
		const string source = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			public interface IMySingleton;

			[Singleton]
			public class MySingleton : IMySingleton { }

			[Singleton<IMySingleton>]
			public class MySingletonGeneric : IMySingleton { }

			public interface IMyTransient;

			[Transient]
			public class MyTransient : IMyTransient { }

			[Transient<IMyTransient>]
			public class MyTransientGeneric : IMyTransient { }

			public interface IMyScoped;

			[Scoped]
			public class MyScoped : IMyScoped { }

			[Scoped<IMyScoped>]
			public class MyScopedGeneric : IMyScoped { }
			""";

		const string updatedSource = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			public interface IMySingleton;

			[Singleton]
			public class MySingleton : IMySingleton { private int myField; }

			[Singleton<IMySingleton>]
			public class MySingletonGeneric : IMySingleton { private int myField; }

			public interface IMyTransient;

			[Transient]
			public class MyTransient : IMyTransient { private int myField; }

			[Transient<IMyTransient>]
			public class MyTransientGeneric : IMyTransient { private int myField; }

			public interface IMyScoped;

			[Scoped]
			public class MyScoped : IMyScoped { private int myField; }

			[Scoped<IMyScoped>]
			public class MyScopedGeneric : IMyScoped { private int myField; }

			public class UnrelatedClass;
			""";

		fixture.RunIncrementalGenerator<ServiceRegistrationGenerator>(source, updatedSource)
			.AssertAllStepsHaveReason(AllTrackedSteps, IncrementalStepRunReason.Cached);
	}

	[Fact]
	public void AllPipelines_AreInvalidated_WhenNewServicesAreAdded()
	{
		const string source = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			public interface IMySingleton;

			[Singleton]
			public class MySingleton : IMySingleton { }

			[Singleton<IMySingleton>]
			public class MySingletonGeneric : IMySingleton { }

			public interface IMyTransient;

			[Transient]
			public class MyTransient : IMyTransient { }

			[Transient<IMyTransient>]
			public class MyTransientGeneric : IMyTransient { }

			public interface IMyScoped;

			[Scoped]
			public class MyScoped : IMyScoped { }

			[Scoped<IMyScoped>]
			public class MyScopedGeneric : IMyScoped { }
			""";

		const string updatedSource = """
			using Nuons.DependencyInjection.Abstractions;

			namespace Nuons.DependencyInjection.Test.Samples;

			public interface IMySingleton;

			[Singleton]
			public class MySingleton : IMySingleton { }

			[Singleton]
			public class MyOtherSingleton : IMySingleton { }

			[Singleton<IMySingleton>]
			public class MySingletonGeneric : IMySingleton { }

			[Singleton<IMySingleton>]
			public class MyOtherSingletonGeneric : IMySingleton { }

			public interface IMyTransient;

			[Transient]
			public class MyTransient : IMyTransient { }

			[Transient]
			public class MyOtherTransient : IMyTransient { }

			[Transient<IMyTransient>]
			public class MyTransientGeneric : IMyTransient { }

			[Transient<IMyTransient>]
			public class MyOtherTransientGeneric : IMyTransient { }

			public interface IMyScoped;

			[Scoped]
			public class MyScoped : IMyScoped { }

			[Scoped]
			public class MyOtherScoped : IMyScoped { }

			[Scoped<IMyScoped>]
			public class MyScopedGeneric : IMyScoped { }

			[Scoped<IMyScoped>]
			public class MyOtherScopedGeneric : IMyScoped { }
			""";

		fixture.RunIncrementalGenerator<ServiceRegistrationGenerator>(source, updatedSource)
			.AssertAllStepsHaveReason(AllTrackedSteps, IncrementalStepRunReason.Modified);
	}
}
