using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nuons.Core.Generators;

namespace Nuons.CodeInjection.Generators;

[Generator]
internal class InjectConstructorGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var injectionProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			KnownCodeInjectionTypes.InjectConstructorAttribute,
			Syntax.IsClassNode,
			ExtractInjectionIncrement
		)
			.Where(increment => increment is not null);

		context.RegisterSourceOutput(injectionProvider, GenerateSources!);
	}

	private InjectionIncrement? ExtractInjectionIncrement(GeneratorAttributeSyntaxContext context, CancellationToken token)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
		{
			return null;
		}

		// Not supporting nested classes for now.
		if (symbol.ContainingType is not null)
		{
			return null;
		}

		var namespaceName = symbol.ToNamespaceSimple();

		var className = symbol.Name;
		if (string.IsNullOrEmpty(className))
		{
			return null;
		}

		var typeParameterNames = symbol.TypeParameters
			.Select(parameter => parameter.Name)
			.ToImmutableArray();

		var fields = GetInjectedFields(symbol);

		return new InjectionIncrement(namespaceName, className, typeParameterNames, fields);
	}

	private static ImmutableArray<InjectedField> GetInjectedFields(INamedTypeSymbol symbol)
	{
		var members = symbol.GetMembers();
		if (members.Length == 0)
		{
			return [];
		}

		var fields = members
			.OfType<IFieldSymbol>()
			.Where(field => HasAttribute(field, KnownCodeInjectionTypes.InjectedAttribute))
			.Select(field => field.ToInjectedField());

		var optionFields = members
			.OfType<IFieldSymbol>()
			.Where(field => HasAttribute(field, KnownCodeInjectionTypes.InjectedOptionsAttribute))
			.Select(field => field.ToInjectedField(true));

		return [..fields, ..optionFields];
	}

	private static bool HasAttribute(IFieldSymbol field, string attributeMetadataName)
		=> field.GetAttributes().Any(attribute => attribute.AttributeClass is not null
			&& attribute.AttributeClass.ToDisplayString() == attributeMetadataName);

	private void GenerateSources(SourceProductionContext context, InjectionIncrement increment)
	{
		var builder = new InjectionSourceBuilder(increment.Namespace, increment.ClassName, increment.TypeParameterNames);
		increment.Fields.ToList().ForEach(builder.With);

		var source = builder.Build();
		var sourceText = SourceText.From(source, Encoding.UTF8);

		// Ensuring unique hint in case of collisions within different namespaces and aritties.
		// Namespace segment is omitted for global-namespace classes to avoid a leading dot.
		var namespacePrefix = string.IsNullOrEmpty(increment.Namespace) ? string.Empty : $"{increment.Namespace}.";
		var hint = $"{namespacePrefix}{increment.ClassName}.{increment.TypeParameterNames.Length}";
		context.AddSource(Sources.GeneratedNameHint(hint), sourceText);
	}
}
