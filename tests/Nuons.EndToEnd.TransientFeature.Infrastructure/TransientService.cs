using Nuons.DependencyInjection.Abstractions;
using Nuons.EndToEnd.TransientFeature.Domain;

namespace Nuons.EndToEnd.TransientFeature.Infrastructure;

[Transient]
internal partial class TransientService : ITransientService
{
	public const string Value = "TransientValue";

	public string GetValue() => Value;
}
