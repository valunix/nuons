using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nuons.DependencyInjection.Generators;

internal static class Syntax
{
	public const string SingleGenericTypeSuffix = "`1";

	private const char NamespaceSeparator = '.';

	public static bool IsClassNode(SyntaxNode node, CancellationToken _) => node is ClassDeclarationSyntax;

	public static AttributeData? FirstOrDefaultAttribute<T>(this ISymbol symbol)
		where T : Attribute
	{
		return symbol.GetAttributes()
			.FirstOrDefault(a => a.AttributeClass?.Name == typeof(T).Name);
	}

	public static string ToNamespace(this ISymbol symbol)
	{
		var namespaceBuilder = new StringBuilder();
		var currentNamespace = symbol.ContainingNamespace;
		while (currentNamespace is not null)
		{
			namespaceBuilder.Insert(0, currentNamespace.Name);
			namespaceBuilder.Insert(0, NamespaceSeparator);
			currentNamespace = currentNamespace.ContainingNamespace;
		}
		return namespaceBuilder.ToString().TrimStart(NamespaceSeparator);
	}

	public static string ToFullName(this ISymbol symbol) =>
		$"{symbol.ToNamespace()}.{symbol.Name}";
}
