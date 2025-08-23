using Nuons.DependencyInjection;
using Nuons.EndToEnd.SingletonFeature.Domain;

namespace Nuons.EndToEnd.Api;

[Transient(typeof(IComplexService))]
internal partial class ComplexService : IComplexService
{
    [Injected]
    private readonly ISingletonService singletonService;

    public string GetValue()
        => singletonService.GetValue();   
}