using Microsoft.CodeAnalysis;
using Nuons.CodeInjection.Abstractions;

namespace Nuons.CodeInjection.Analyzers;

// TODO lazy loading
internal class CodeInjectionAnalyzerContext(Compilation compilation)
{
	public INamedTypeSymbol InjectConstructorAttributes { get; init; } = compilation.GetTypeByMetadataName(typeof(InjectConstructorAttribute).FullName)!;

	public INamedTypeSymbol[] InjectedAttributes { get; init; } =
	[
		compilation.GetTypeByMetadataName(typeof(InjectedAttribute).FullName)!,
		compilation.GetTypeByMetadataName(typeof(InjectedOptionsAttribute).FullName)!,
	];
}
