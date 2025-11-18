using Microsoft.CodeAnalysis;
using Nuons.Core.Generators;

namespace Nuons.DependencyInjection.Generators.Injection;

internal static class InjectedFieldExtensions
{
	public static InjectedField ToInjectedField(this IFieldSymbol field, bool isOptionsValue = false)
	{
		var type = field.Type.ToFullTypeName();
		var name = field.Name;
		return new InjectedField(type, name, isOptionsValue);
	}
}
