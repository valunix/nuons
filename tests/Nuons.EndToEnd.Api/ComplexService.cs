using Nuons.DependencyInjection.Abstractions;
using Nuons.EndToEnd.SingletonFeature.Domain;

namespace Nuons.EndToEnd.Api;

[Transient]
internal partial class ComplexService : IComplexService
{
	[Injected]
	private readonly ISingletonService singletonService;

	public string GetValue() => singletonService.GetValue();
}
