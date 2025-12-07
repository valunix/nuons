using Microsoft.CodeAnalysis;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Analyzers;

// TODO lazy loading
internal class DependencyInjectionAnalyzerContext(Compilation compilation)
{
	public INamedTypeSymbol[] ServiceAttributes { get; init; } =
	[
		// TODO generic versions
		compilation.GetTypeByMetadataName(typeof(SingletonAttribute).FullName)!,
		compilation.GetTypeByMetadataName(typeof(ScopedAttribute).FullName)!,
		compilation.GetTypeByMetadataName(typeof(TransientAttribute).FullName)!,
	];
}
