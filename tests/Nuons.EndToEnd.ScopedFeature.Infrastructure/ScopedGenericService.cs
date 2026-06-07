using Nuons.DependencyInjection.Abstractions;
using Nuons.EndToEnd.ScopedFeature.Domain;

namespace Nuons.EndToEnd.ScopedFeature.Infrastructure;

[Scoped<IScopedGenericService>]
internal class ScopedGenericService : IScopedGenericService
{
	public const string Value = "ScopedGenericValue";

	public string GetValue() => Value;
}
