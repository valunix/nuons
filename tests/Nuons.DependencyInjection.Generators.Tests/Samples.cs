using Microsoft.Extensions.Options;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators.Tests;

public interface ISingletonService;

[Singleton]
internal partial class SingletonService : ISingletonService
{
	[InjectedOptions] private readonly SampleOptions options;
}

[Singleton<ISingletonService>]
internal partial class SingletonServiceGeneric : ISingletonService
{
	[InjectedOptions] private readonly SampleOptions options;
}

public interface IScopedService;

[Scoped]
internal partial class ScopedService : IScopedService
{
	[Injected] private readonly ISingletonService singletonField;
}

[Scoped<IScopedService>]
internal partial class ScopedServiceGeneric : IScopedService
{
	[Injected] private readonly ISingletonService singletonField;
}

public interface ITransientService;

[Transient]
internal partial class TransientService : ITransientService
{
	[Injected] private readonly IScopedService scopedField;
}

[Transient<ITransientService>]
internal partial class TransientServiceGeneric : ITransientService
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

[Scoped]
internal partial class ServiceWithOptions
{
	[Injected] private readonly ISingletonService singletonField;

	[Injected] private readonly IOptions<SampleOptions> options;

	[InjectedOptions] private readonly SampleOptions sampleOptions;
}

[InjectedConstructor]
internal partial class InjectedConstructorService
{
	[Injected] private readonly IAmGeneric<string?> genericField;
	[Injected] private readonly ISingletonService singletonField;
	[InjectedOptions] private readonly SampleOptions sampleOptions;
}

internal interface IAmGeneric<T>;

[RootRegistration(typeof(RootRegistration))]
internal partial class RootRegistration;

internal interface IBase;
internal interface ITarget : IBase;
[Singleton]
internal partial class MultipleInterfacesIndirect : ITarget;
[Singleton]
internal partial class MultipleInterfacesDirect : ITarget, IBase;
