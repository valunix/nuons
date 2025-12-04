using Microsoft.Extensions.Options;
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Generators.Tests;

public interface ITestService;

internal class SampleOptions;

internal interface IGenericService<T>;

[InjectConstructor]
internal partial class InjectedConstructorService
{
	[Injected] private readonly IGenericService<string?> genericField;
	[Injected] private readonly ITestService testField;
	[InjectedOptions] private readonly SampleOptions sampleOptions;
}
