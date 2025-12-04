using Microsoft.CodeAnalysis;

namespace Nuons.CodeInjection.Analyzers;

// TODO lazy loading
internal class CodeInjectionAnalyzerContext(Compilation compilation)
{
	public INamedTypeSymbol InjectConstructorAttributes { get; init; } = compilation.GetTypeByMetadataName("Nuons.CodeInjection.Abstractions.InjectConstructorAttribute")!;

	public INamedTypeSymbol[] InjectedAttributes { get; init; } =
	[
		compilation.GetTypeByMetadataName("Nuons.CodeInjection.Abstractions.InjectedAttribute")!,
		compilation.GetTypeByMetadataName("Nuons.CodeInjection.Abstractions.InjectedOptionsAttribute")!,
	];
}
