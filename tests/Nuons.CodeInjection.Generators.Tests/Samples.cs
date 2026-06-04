using Microsoft.Extensions.Options;
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

public interface ITestService;

internal class SampleOptions;

internal interface IGenericService<T>;

[InjectConstructor]
internal partial class InjectedConstructorService
{
	[Injected] private readonly IGenericService<string?> genericField = null!;
	[Injected] private readonly ITestService testField = null!;
	[InjectedOptions] private readonly SampleOptions sampleOptions = null!;
}

[InjectConstructor]
internal partial class GenericInjectedConstructorService<TFirst, TSecond>
	where TFirst : class
{
	[Injected] private readonly IGenericService<TFirst> genericField = null!;
	[Injected] private readonly ITestService testField = null!;
}

// Same class name, different arity — must not collide on the generated hint name.
[InjectConstructor]
internal partial class GenericInjectedConstructorService<TFirst>
	where TFirst : class
{
	[Injected] private readonly IGenericService<TFirst> genericField = null!;
}

// Nested classes should be skipped.
internal partial class OuterHost
{
	[InjectConstructor]
	internal partial class NestedInjectedConstructorService
	{
		[Injected] private readonly ITestService testField = null!;
	}
}
