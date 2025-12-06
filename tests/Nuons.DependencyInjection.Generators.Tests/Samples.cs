using Microsoft.Extensions.Options;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators.Tests;

public interface ITestService;

[Singleton]
internal partial class SingletonService : ITestService;

[Singleton<ITestService>]
internal partial class SingletonServiceGeneric : ITestService;

[Scoped]
internal partial class ScopedService : ITestService;

[Scoped<ITestService>]
internal partial class ScopedServiceGeneric : ITestService;

[Transient]
internal partial class TransientService : ITestService;

[Transient<ITestService>]
internal partial class TransientServiceGeneric : ITestService;

[Options(nameof(SampleOptions))]
internal class SampleOptions;

[RootRegistration(typeof(RootRegistration))]
internal partial class RootRegistration;

internal interface IBase;
internal interface ITarget : IBase;

[Singleton]
internal partial class MultipleInterfacesIndirect : ITarget;

[Singleton]
internal partial class MultipleInterfacesDirect : ITarget, IBase;
