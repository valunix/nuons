using Nuons.DependencyInjection;
using Nuons.EndToEnd.ScopedFeature.Domain;

namespace Nuons.EndToEnd.ScopedFeature.Infrastructure;

[Scoped(typeof(IScopedService))]
internal partial class ScopedService : IScopedService
{
    public const string Value = "ScopedValue";
        
    public string GetValue() => Value;
} 