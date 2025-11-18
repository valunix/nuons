using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nuons.Core.Generators;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators.Injection;

internal abstract class InjectionGenerator<T> : IIncrementalGenerator
	where T : Attribute
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var injectionProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(T).FullName,
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

		var namespaceName = symbol.ToNamespaceSimple();
		if (string.IsNullOrEmpty(namespaceName))
		{
			return null;
		}

		var className = symbol.Name;
		if (string.IsNullOrEmpty(className))
		{
			return null;
		}

		var members = symbol.GetMembers();
		if (!members.Any())
		{
			return null;
		}

		var fields = members
			.OfType<IFieldSymbol>()
			.Where(field => field.GetAttributes()
				.Any(attribute => attribute.AttributeClass is not null
					&& attribute.AttributeClass.Name == nameof(InjectedAttribute)))
			.Select(field => field.ToInjectedField())
			.ToList();

		var optionFields = members
			.OfType<IFieldSymbol>()
			.Where(field => field.GetAttributes()
				.Any(attribute => attribute.AttributeClass is not null
					&& attribute.AttributeClass.Name == nameof(InjectedOptionsAttribute)))
			.Select(field => field.ToInjectedField(true))
			.ToList();

		fields.AddRange(optionFields);
		if (!fields.Any())
		{
			return null;
		}

		var accessibility = GetAccessibility(symbol.DeclaredAccessibility);

		return new InjectionIncrement(namespaceName, className, accessibility, [.. fields]);
	}

	private static string GetAccessibility(Accessibility accessibility)
	{
		return accessibility switch
		{
			Accessibility.Internal => "internal",
			Accessibility.Public => "public",
			_ => string.Empty,
		};
	}

	private void GenerateSources(SourceProductionContext context, InjectionIncrement increment)
	{
		var builder = new InjectionSourceBuilder(increment.Namespace, increment.ClassName, increment.Accessibility);
		increment.Fields.ToList().ForEach(builder.With);

		var source = builder.Build();
		var sourceText = SourceText.From(source, Encoding.UTF8);

		context.AddSource(Sources.GeneratedNameHint($"{increment.ClassName}"), sourceText);
	}
}
