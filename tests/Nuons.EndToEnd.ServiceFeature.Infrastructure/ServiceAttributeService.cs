using Nuons.DependencyInjection.Abstractions;
using Nuons.EndToEnd.ServiceFeature.Domain;

namespace Nuons.EndToEnd.ServiceFeature.Infrastructure;

[Service(Lifetime.Singleton, typeof(IServiceAttributeService))]
internal partial class ServiceAttributeService : IServiceAttributeService
{
	public const string Value = "ServiceValue";

	public string GetValue() => Value;
}
