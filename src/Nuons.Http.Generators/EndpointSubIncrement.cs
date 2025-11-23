using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Nuons.Http.Generators;

internal record EndpointSubIncrement(ImmutableArray<INamedTypeSymbol> Symbols, EndpointAttributes? Attributes);
