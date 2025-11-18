using Nuons.DependencyInjection.Abstractions;
using Nuons.EndToEnd.ScopedFeature.Domain;

namespace Nuons.EndToEnd.ScopedFeature.Infrastructure;

[Scoped]
internal partial class ScopedService : IScopedService
{
	public const string Value = "ScopedValue";

	public string GetValue() => Value;
}
