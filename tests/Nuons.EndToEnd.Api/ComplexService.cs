using Nuons.CodeInjection.Abstractions;
using Nuons.DependencyInjection.Abstractions;
using Nuons.EndToEnd.SingletonFeature.Domain;

namespace Nuons.EndToEnd.Api;

[Transient]
[InjectConstructor]
internal partial class ComplexService : IComplexService
{
	[Injected]
	private readonly ISingletonService singletonService;

	public string GetValue() => singletonService.GetValue();
}
