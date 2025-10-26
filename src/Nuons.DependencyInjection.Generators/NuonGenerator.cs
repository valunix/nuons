using Microsoft.CodeAnalysis;
namespace Nuons.DependencyInjection.Generators;

internal abstract class NuonGenerator<TIncrement> : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var provider = CreateIncrementProvider(context);
		context.RegisterSourceOutput(provider, GenerateSources);
	}

	protected abstract IncrementalValueProvider<TIncrement> CreateIncrementProvider(IncrementalGeneratorInitializationContext context);

	protected abstract void GenerateSources(SourceProductionContext context, TIncrement increment);
}
