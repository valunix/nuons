using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nuons.Core.Generators;

public static class Syntax
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

	// TODO avoid trim start?
	public static string ToNamespaceSimple(this ISymbol symbol)
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

	// TODO handle guid?
	public static string ToFullTypeName(this ITypeSymbol symbol)
	{
		var format = new SymbolDisplayFormat(
			globalNamespaceStyle:
				SymbolDisplayGlobalNamespaceStyle.Included,
			typeQualificationStyle:
				SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
			genericsOptions:
				SymbolDisplayGenericsOptions.IncludeTypeParameters
				| SymbolDisplayGenericsOptions.IncludeVariance,
			miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
				| SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
				| SymbolDisplayMiscellaneousOptions.ExpandNullable
		);

		return symbol.ToDisplayString(format);
	}

	// TODO
	public static IncrementalValuesProvider<TValues> WhereNotNull<TValues>(this IncrementalValuesProvider<TValues?> provider)
	{
		return provider
			.Where(increment => increment is not null)
			.Select( static (increment, _) => increment!);
	}

	public static bool HasAttribute(this ISymbol assembly, INamedTypeSymbol markerAttribute)
		=> assembly.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, markerAttribute));

	public static IEnumerable<INamedTypeSymbol> GetAllTypes(this IAssemblySymbol assembly)
	{
		return GetTypesRecursive(assembly.GlobalNamespace);
	}

	private static IEnumerable<INamedTypeSymbol> GetTypesRecursive(INamespaceSymbol namespaceSymbol)
	{
		foreach (var type in namespaceSymbol.GetTypeMembers())
		{
			foreach (var nestedType in GetNestedTypes(type))
			{
				yield return nestedType;
			}
		}

		foreach (var subNamespaces in namespaceSymbol.GetNamespaceMembers())
		{
			foreach (var type in GetTypesRecursive(subNamespaces))
			{
				yield return type;
			}
		}
	}

	private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol currentType)
	{
		yield return currentType;

		foreach (var type in currentType.GetTypeMembers())
		{
			foreach (var nestedType in GetNestedTypes(type))
			{
				yield return nestedType;
			}
		}
	}
}
