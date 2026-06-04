using Nuons.DependencyInjection.Abstractions;
using Nuons.EndToEnd.TransientFeature.Domain;

namespace Nuons.EndToEnd.TransientFeature.Infrastructure;

[Transient<ITransientGenericService>]
internal class TransientGenericService : ITransientGenericService
{
	public const string Value = "TransientGenericValue";

	public string GetValue() => Value;
}
