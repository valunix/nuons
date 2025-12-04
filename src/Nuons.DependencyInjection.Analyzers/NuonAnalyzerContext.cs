using Microsoft.CodeAnalysis;

namespace Nuons.DependencyInjection.Analyzers;

// TODO lazy loading, avoid duplication
internal class NuonAnalyzerContext(Compilation compilation)
{
	public INamedTypeSymbol[] ParameterlessServiceAttributes { get; init; } =
	[
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.SingletonAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.ScopedAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.TransientAttribute")!,
	];

	public INamedTypeSymbol[] ServiceAttributes { get; init; } =
	[
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.SingletonAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.ScopedAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.TransientAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.InjectedConstructorAttribute")!,
	];

	public INamedTypeSymbol[] InjectedAttributes { get; init; } =
	[
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.InjectedAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.InjectedOptionsAttribute")!,
	];
}
