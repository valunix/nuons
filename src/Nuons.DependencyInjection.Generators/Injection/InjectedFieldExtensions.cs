using Microsoft.CodeAnalysis;

namespace Nuons.DependencyInjection.Generators.Injection;

internal static class InjectedFieldExtensions
{
	public static InjectedField ToInjectedField(this IFieldSymbol field, bool isOptionsValue = false)
	{
		var type = field.Type is INamedTypeSymbol namedSymbol && namedSymbol.IsGenericType
			? field.Type.ToDisplayString()
			: field.Type.ToFullName();
		var name = field.Name;
		return new InjectedField(type, name, isOptionsValue);
	}
}
