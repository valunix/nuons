using Microsoft.CodeAnalysis;

namespace Nuons.DependencyInjection.Analyzers;

internal class NuonAnalyzerContext(Compilation compilation)
{
	public INamedTypeSymbol[] ServiceAttributes { get; init; } =
	[
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.ServiceAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.SingletonAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.ScopedAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.TransientAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.InjectedConstructorAttribute")!,
	];

	public INamedTypeSymbol[] InjectedAttributes { get; init; } =
	[
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.InjectedAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.InjectedOptionsAttribute")!,
	];
}
