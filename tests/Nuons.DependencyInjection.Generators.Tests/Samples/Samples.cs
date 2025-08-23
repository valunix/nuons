using Microsoft.Extensions.Options;

namespace Nuons.DependencyInjection.Generators.Tests.Samples;

public static class Samples
{
	public const string Path = "../../../Samples/Samples.cs";

	public static string Load() =>
		File.ReadAllText(Path);
}

public interface ISingletonService;

[Singleton(typeof(ISingletonService))]
internal partial class SingletonService : ISingletonService
{
	[InjectedOptions] private readonly SampleOptions options;
}

public interface IScopedService;

[Scoped(typeof(IScopedService))]
internal partial class ScopedService : IScopedService
{
	[Injected] private readonly ISingletonService singletonField;
}

public interface ITransientService;

[Transient(typeof(ITransientService))]
internal partial class TransientService : ITransientService
{
	[Injected] private readonly IScopedService scopedField;
}

public interface ILifetimeService;

//[Service((Lifetime)42, typeof(ILifetimeService))]
[Service(Lifetime.Transient, typeof(ILifetimeService))]
internal partial class LifetimeService : ILifetimeService
{
	[Injected] private readonly ISingletonService singletonField;

	[Injected] private readonly IScopedService scopedField;

	[Injected] private readonly ITransientService transientField;
}

[Options(nameof(SampleOptions))]
internal class SampleOptions;

[Scoped(typeof(ServiceWithOptions))]
internal partial class ServiceWithOptions
{
	[Injected] private readonly ISingletonService singletonField;

	[Injected] private readonly IOptions<SampleOptions> options;

	[InjectedOptions] private readonly SampleOptions sampleOptions;
}

[RootRegistration(typeof(Samples))]
internal partial class RootRegistration { }