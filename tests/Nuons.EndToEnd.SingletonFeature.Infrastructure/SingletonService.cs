using Nuons.DependencyInjection;
using Nuons.EndToEnd.SingletonFeature.Domain;

namespace Nuons.EndToEnd.SingletonFeature.Infrastructure;

[Singleton(typeof(ISingletonService))]
internal partial class SingletonService : ISingletonService
{
    public const string Value = "SingletonValue";
        
    public string GetValue() => Value;
}