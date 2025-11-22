using Microsoft.CodeAnalysis;

namespace Nuons.Http.Generators;

internal record EndpointGeneratorContext(
	INamedTypeSymbol GetAttributeSymbol,
	INamedTypeSymbol PostAttributeSymbol
);
