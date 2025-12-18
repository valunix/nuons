using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nuons.Core.Generators;

public static class Syntax
{
	public const string SingleGenericTypeSuffix = "`1";

	private const char NamespaceSeparator = '.';

	public static bool IsClassNode(SyntaxNode node, CancellationToken _) => node is ClassDeclarationSyntax;

	public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol)
		=> symbol.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol));

	public static AttributeData? FirstOrDefaultAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol)
		=> symbol.GetAttributes().FirstOrDefault(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol));
	public static AttributeData? FirstOrDefaultAttribute<T>(this ISymbol symbol) where T : Attribute
		=> symbol.FirstOrDefaultAttribute(typeof(T).FullName);

	public static AttributeData? FirstOrDefaultAttribute(this ISymbol symbol, string attributeFullName)
		=> symbol.GetAttributes().FirstOrDefault(attribute => attribute.AttributeClass?.ToFullTypeName(false, false) == attributeFullName);

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

	public static string ToFullTypeName(this ITypeSymbol symbol, bool useGlobal = true, bool useGeneric = true)
	{
		var format = new SymbolDisplayFormat(
			globalNamespaceStyle: useGlobal
				? SymbolDisplayGlobalNamespaceStyle.Included
				: SymbolDisplayGlobalNamespaceStyle.Omitted,
			typeQualificationStyle:
				SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
			genericsOptions: useGeneric
				? SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance
				: SymbolDisplayGenericsOptions.None,
			miscellaneousOptions:
				SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
				| SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
				| SymbolDisplayMiscellaneousOptions.ExpandNullable
		);

		return symbol.ToDisplayString(format);
	}

	public static IncrementalValuesProvider<TValues> WhereNotNull<TValues>(this IncrementalValuesProvider<TValues?> provider)
	{
		return provider
			.Where(increment => increment is not null)
			.Select(static (increment, _) => increment!);
	}

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
