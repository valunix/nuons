using Nuons.DependencyInjection.Abstractions;
using Nuons.EndToEnd.SingletonFeature.Domain;

namespace Nuons.EndToEnd.SingletonFeature.Infrastructure;

[Singleton]
internal partial class SingletonService : ISingletonService
{
	public const string Value = "SingletonValue";

	public string GetValue() => Value;
}
