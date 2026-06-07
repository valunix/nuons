using Microsoft.CodeAnalysis;

namespace Nuons.DependencyInjection.Analyzers;

internal static class DependencyInjectionSymbolExtensions
{
	public static List<AttributeData> GetServiceAttributes(this ISymbol symbol, DependencyInjectionAnalyzerContext context)
		=> symbol.GetAttributes()
			.Where(attribute => attribute.AttributeClass is not null
				&& context.ServiceAttributes.Contains(attribute.AttributeClass.OriginalDefinition, SymbolEqualityComparer.Default))
			.ToList();
}
