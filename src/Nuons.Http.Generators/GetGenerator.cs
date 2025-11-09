using Microsoft.CodeAnalysis;
using Nuons.Core.Generators;
using Nuons.Http.Abstractions;

namespace Nuons.Http.Generators;

[Generator]
internal class GetGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var optionsProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(RouteAttribute).FullName,
			Syntax.IsClassNode,
			ExtractEndpoints
		)
			.Where(optionDefinition => optionDefinition is not null)
			.Collect();
	}

	private HttpEndpointIncrement? ExtractEndpoints(GeneratorAttributeSyntaxContext context, CancellationToken token)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
		{
			return null;
		}

		var attribute = symbol.FirstOrDefaultAttribute<RouteAttribute>();
		if (attribute is null)
		{
			return null;
		}

		var constructorArguments = attribute.ConstructorArguments;
		if (constructorArguments.Length is not 1)
		{
			return null;
		}

		var members = symbol.GetMembers()
			.OfType<IMethodSymbol>()
			.Where(method => method.GetAttributes()
				.Any(attribute => attribute.AttributeClass is not null
					&& attribute.AttributeClass.Name == nameof(GetAttribute)))
			.ToList();

		return null;
	}
}
