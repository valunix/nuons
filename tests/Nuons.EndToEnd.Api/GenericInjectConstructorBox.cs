using Nuons.CodeInjection.Abstractions;
using Nuons.EndToEnd.SingletonFeature.Domain;

namespace Nuons.EndToEnd.Api;

[InjectConstructor]
internal partial class GenericInjectConstructorBox<T>
{
	[Injected]
	private readonly ISingletonService singletonService;

	public string Describe() => $"{typeof(T).Name}:{singletonService.GetValue()}";
}
