using Nuons.DependencyInjection;
using Nuons.EndToEnd.TransientFeature.Domain;

namespace Nuons.EndToEnd.TransientFeature.Infrastructure;

[Transient(typeof(ITransientService))]
internal partial class TransientService : ITransientService
{
	public const string Value = "TransientValue";

	public string GetValue() => Value;
}
