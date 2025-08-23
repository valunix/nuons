using Microsoft.CodeAnalysis;

namespace Nuons.DependencyInjection.Analyzers;

internal class NuonAnalyzerContext
{
	public INamedTypeSymbol[] ServiceAttributes { get; init; }
	public INamedTypeSymbol[] InjectedAttributes { get; init; }

	public NuonAnalyzerContext(Compilation compilation)
	{
		ServiceAttributes =
		[
			compilation.GetTypeByMetadataName("Nuons.DependencyInjection.ServiceAttribute"),
			compilation.GetTypeByMetadataName("Nuons.DependencyInjection.SingletonAttribute"),
			compilation.GetTypeByMetadataName("Nuons.DependencyInjection.ScopedAttribute"),
			compilation.GetTypeByMetadataName("Nuons.DependencyInjection.TransientAttribute"),
		];

		InjectedAttributes =
		[
			compilation.GetTypeByMetadataName("Nuons.DependencyInjection.InjectedAttribute"),
			compilation.GetTypeByMetadataName("Nuons.DependencyInjection.InjectedOptionsAttribute"),
		];
	}
}
