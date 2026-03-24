using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Nuons.Core.Generators;

namespace Nuons.CodeInjection.Generators;

[Generator]
internal class DtoMappingGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
			KnownCodeInjectionTypes.MapFromAttribute,
			IsRecordNode,
			ExtractMappingIncrement
		).Where(increment => increment is not null);

		context.RegisterSourceOutput(provider, GenerateSources!);
	}

	private static bool IsRecordNode(SyntaxNode node, CancellationToken _) => node is RecordDeclarationSyntax;

	private MappingIncrement? ExtractMappingIncrement(GeneratorAttributeSyntaxContext context, CancellationToken token)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
			return null;

		if (!symbol.IsRecord)
			return null;

		if (context.TargetNode is not RecordDeclarationSyntax recordSyntax)
			return null;

		if (!recordSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
			return null;

		var namespaceName = symbol.ToNamespaceSimple();
		if (string.IsNullOrEmpty(namespaceName))
			return null;

		var recordName = symbol.Name;
		var accessibility = symbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";

		// Extract source type from [MapFrom(typeof(T))]
		var attributeData = context.Attributes[0];
		if (attributeData.ConstructorArguments.Length == 0)
			return null;

		if (attributeData.ConstructorArguments[0].Value is not INamedTypeSymbol sourceType)
			return null;

		var sourceFullTypeName = sourceType.ToFullTypeName();
		var sourceSimpleName = sourceType.Name;

		// Collect property names to ignore from [MapIgnore("PropertyName")]
		var ignoredProperties = new HashSet<string>();
		foreach (var attr in symbol.GetAttributes())
		{
			if (attr.AttributeClass?.ToFullTypeName(false, false) == KnownCodeInjectionTypes.MapIgnoreAttribute
				&& attr.ConstructorArguments.Length > 0
				&& attr.ConstructorArguments[0].Value is string propertyName)
			{
				ignoredProperties.Add(propertyName);
			}
		}

		// Extract and filter source properties
		var properties = sourceType.GetMembers()
			.OfType<IPropertySymbol>()
			.Where(p => p.DeclaredAccessibility == Accessibility.Public)
			.Where(p => p.GetMethod is not null)
			.Where(p => !ignoredProperties.Contains(p.Name))
			.Where(p => IsSupportedType(p.Type))
			.Select(p => new MappedProperty(p.Name, p.Type.ToFullTypeName()))
			.ToImmutableArray();

		return new MappingIncrement(namespaceName, recordName, accessibility, sourceFullTypeName, sourceSimpleName, properties);
	}

	private static bool IsSupportedType(ITypeSymbol type)
	{
		// Unwrap Nullable<T>
		if (type is INamedTypeSymbol namedType
			&& namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
		{
			return IsSupportedType(namedType.TypeArguments[0]);
		}

		if (type.TypeKind == TypeKind.Enum)
			return true;

		return type.SpecialType switch
		{
			SpecialType.System_Boolean => true,
			SpecialType.System_Byte => true,
			SpecialType.System_SByte => true,
			SpecialType.System_Int16 => true,
			SpecialType.System_UInt16 => true,
			SpecialType.System_Int32 => true,
			SpecialType.System_UInt32 => true,
			SpecialType.System_Int64 => true,
			SpecialType.System_UInt64 => true,
			SpecialType.System_Single => true,
			SpecialType.System_Double => true,
			SpecialType.System_Decimal => true,
			SpecialType.System_Char => true,
			SpecialType.System_String => true,
			_ => IsWellKnownStructType(type)
		};
	}

	private static bool IsWellKnownStructType(ITypeSymbol type)
	{
		var fullName = type.ToDisplayString();
		return fullName is "System.DateTime"
			or "System.DateTimeOffset"
			or "System.DateOnly"
			or "System.TimeOnly"
			or "System.TimeSpan"
			or "System.Guid";
	}

	private void GenerateSources(SourceProductionContext context, MappingIncrement increment)
	{
		// File 1: Partial record with properties
		var recordBuilder = new MappingRecordSourceBuilder(
			increment.TargetNamespace, increment.TargetRecordName, increment.TargetAccessibility);
		foreach (var property in increment.Properties)
			recordBuilder.With(property);
		var recordSource = SourceText.From(recordBuilder.Build(), Encoding.UTF8);
		context.AddSource(Sources.GeneratedNameHint(increment.TargetRecordName), recordSource);

		// File 2: Extension method class
		var extensionBuilder = new MappingExtensionSourceBuilder(
			increment.TargetNamespace, increment.TargetRecordName,
			increment.TargetAccessibility, increment.SourceFullTypeName);
		foreach (var property in increment.Properties)
			extensionBuilder.With(property);
		var extensionSource = SourceText.From(extensionBuilder.Build(), Encoding.UTF8);
		context.AddSource(Sources.GeneratedNameHint($"{increment.TargetRecordName}Extensions"), extensionSource);
	}
}
