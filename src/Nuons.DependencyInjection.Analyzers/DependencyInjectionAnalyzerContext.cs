using Microsoft.CodeAnalysis;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Analyzers;

internal class DependencyInjectionAnalyzerContext(Compilation compilation)
{
	public INamedTypeSymbol[] ServiceAttributes { get; init; } =
	[
		compilation.GetTypeByMetadataName(typeof(SingletonAttribute).FullName)!,
		compilation.GetTypeByMetadataName(typeof(ScopedAttribute).FullName)!,
		compilation.GetTypeByMetadataName(typeof(TransientAttribute).FullName)!,
		compilation.GetTypeByMetadataName(typeof(SingletonAttribute<>).FullName!)!,
		compilation.GetTypeByMetadataName(typeof(ScopedAttribute<>).FullName!)!,
		compilation.GetTypeByMetadataName(typeof(TransientAttribute<>).FullName!)!,
	];

	public INamedTypeSymbol OptionsAttribute { get; init; } = compilation.GetTypeByMetadataName(typeof(OptionsAttribute).FullName)!;
}
