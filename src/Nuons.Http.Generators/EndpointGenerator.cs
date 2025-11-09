using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Nuons.Core.Generators;
using Nuons.Http.Abstractions;

namespace Nuons.Http.Generators;

[Generator]
internal class EndpointGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(RouteAttribute).FullName,
			Syntax.IsClassNode,
			ExtractEndpoints
		)
			.Where(optionDefinition => optionDefinition is not null)
			.Collect();

		context.RegisterSourceOutput(provider, GenerateSources);
	}

	private void GenerateSources(SourceProductionContext context, ImmutableArray<EndpointIncrement> increments)
	{
		if(increments.Length == 0)
		{
			return;
		}

		var builder = new EndpointSourceBuilder(increments);
		var source = builder.Build();
		context.AddSource("NuonEndpoints.g.cs", source);
	}

	private EndpointIncrement? ExtractEndpoints(GeneratorAttributeSyntaxContext context, CancellationToken token)
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

		// TODO
		var constructorArguments = attribute.ConstructorArguments;
		if (constructorArguments.Length is not 1)
		{
			return null;
		}

		var route = constructorArguments[0].Value as string;
		if(route is null)
		{
			return null;
		}

		var members = symbol.GetMembers()
			.OfType<IMethodSymbol>()
			.Where(method => method.GetAttributes()
				.Any(attribute => attribute.AttributeClass is not null
					&& attribute.AttributeClass.Name == nameof(GetAttribute)))
			.ToList();
		if(members.Count == 0)
		{
			return null;
		}

		var implementationType = symbol.ToFullName();
		var implementationMethod = members.First().Name;

		var increment = new EndpointIncrement("Get", route, implementationType, implementationMethod);
		return increment;
	}
}
