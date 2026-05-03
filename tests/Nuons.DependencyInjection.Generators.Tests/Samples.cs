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

[Options(nameof(SampleOptionsValidated), Validate = true)]
internal class SampleOptionsValidated;

[Options(nameof(SampleOptionsValidatedOnStart), ValidateOnStart = true)]
internal class SampleOptionsValidatedOnStart;

[Options(nameof(SampleOptionsValidatedAndValidatedOnStart), Validate = true, ValidateOnStart = true)]
internal class SampleOptionsValidatedAndValidatedOnStart;

internal interface IBase;
internal interface ITarget : IBase;

[Singleton]
internal partial class MultipleInterfacesIndirect : ITarget;

[Singleton]
internal partial class MultipleInterfacesDirect : ITarget, IBase;
