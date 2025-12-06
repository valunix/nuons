using Microsoft.CodeAnalysis;

namespace Nuons.DependencyInjection.Analyzers;

// TODO lazy loading
internal class DependencyInjectionAnalyzerContext(Compilation compilation)
{
	public INamedTypeSymbol[] ServiceAttributes { get; init; } =
	[
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.SingletonAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.ScopedAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.DependencyInjection.Abstractions.TransientAttribute")!,
	];
}
