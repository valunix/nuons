using Nuons.DependencyInjection;
using Nuons.EndToEnd.SingletonFeature.Domain;

namespace Nuons.EndToEnd.SingletonFeature.Infrastructure;

[Singleton<ISingletonGenericService>]
internal partial class SingletonGenericService : ISingletonGenericService
{
	public const string Value = "SingletonGenericValue";

	public string GetValue() => Value;
}
