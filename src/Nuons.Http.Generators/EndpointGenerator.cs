using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Nuons.Core.Abstractions;
using Nuons.Core.Generators;
using Nuons.Http.Abstractions;

namespace Nuons.Http.Generators;

[Generator]
internal class EndpointGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var currentProjectProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(RouteAttribute).FullName,
			Syntax.IsClassNode,
			ExtractIncrement
		)
			.Where(increment => increment is not null)
			.Collect();

		var referencesProvider = context.CompilationProvider
			.SelectMany(ExtractEndpointsFromReferences)
			.Collect();

		var mergedProvider = currentProjectProvider
			.Combine(referencesProvider)
			.Select(static (combined, ct) => combined.Left.AddRange(combined.Right));

		context.RegisterSourceOutput(mergedProvider, GenerateSources);
	}

	private EndpointIncrement? ExtractIncrement(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
		{
			return null;
		}

		// TODO optimize
		var getAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(GetAttribute).FullName);
		if(getAttributeSymbol is null)
		{
			return null;
		}

		return ExtractIncrement(symbol, getAttributeSymbol, cancellationToken);
	}

	private static EndpointIncrement? ExtractIncrement(INamedTypeSymbol symbol, INamedTypeSymbol getAttributeSymbol, CancellationToken cancellationToken)
	{
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
		if (route is null)
		{
			return null;
		}

		var implementationType = symbol.ToFullTypeName();
		var handler = new HandlerRegistration(implementationType);

		var members = symbol.GetMembers()
			.OfType<IMethodSymbol>()
			.Where(method => method.HasAttribute(getAttributeSymbol))
			.ToList();
		if (members.Count == 0)
		{
			return null;
		}

		var endpointBuilder = ImmutableArray.CreateBuilder<EndpointRegistration>();
		foreach (var member in members)
		{
			var parameters = member.Parameters;
			var parameterBuilder = ImmutableArray.CreateBuilder<MethodParameter>();
			foreach (var parameter in parameters)
			{
				var typeName = parameter.Type.ToFullTypeName();
				var name = parameter.Name;
				parameterBuilder.Add(new MethodParameter(typeName, name));
			}
			var implementationMethod = member.Name;
			var endpoint = new EndpointRegistration(HttpMethods.Get, route, implementationMethod, parameterBuilder.ToImmutable());
			endpointBuilder.Add(endpoint);
		}

		//var increment = EndpointIncrement.NewGetEndpoint(route, implementationType, implementationMethod, builder.ToImmutable());
		var increment = new EndpointIncrement(handler, endpointBuilder.ToImmutable());
		return increment;
	}

	private static ImmutableArray<EndpointIncrement> ExtractEndpointsFromReferences(Compilation compilation, CancellationToken cancellationToken)
	{
		var results = ImmutableArray.CreateBuilder<EndpointIncrement>();

		var routeAttribute = compilation.GetTypeByMetadataName(typeof(RouteAttribute).FullName!);
		if (routeAttribute is null)
		{
			return [];
		}

		var nuonMarkerAttribute = compilation.GetTypeByMetadataName(typeof(AssemblyHasNuons).FullName!);
		if (nuonMarkerAttribute is null)
		{
			return [];
		}

		var getAttributeSymbol = compilation.GetTypeByMetadataName(typeof(GetAttribute).FullName);
		if (getAttributeSymbol is null)
		{
			return [];
		}

		foreach (var assemblySymbol in compilation.SourceModule.ReferencedAssemblySymbols)
		{
			if (!assemblySymbol.HasAttribute(nuonMarkerAttribute))
			{
				continue;
			}

			CollectAssemblyTypes(assemblySymbol, routeAttribute, getAttributeSymbol, results, cancellationToken);
		}

		return results.ToImmutable();
	}

	private static void CollectAssemblyTypes(IAssemblySymbol assemblySymbol, INamedTypeSymbol routeAttribute, INamedTypeSymbol getAttributeSymbol, ImmutableArray<EndpointIncrement>.Builder sink, CancellationToken cancellationToken)
	{
		foreach (var type in assemblySymbol.GetAllTypes())
		{
			if (!type.HasAttribute(routeAttribute))
			{
				continue;
			}

			var increment = ExtractIncrement(type, getAttributeSymbol, cancellationToken);
			if (increment is not null)
			{
				sink.Add(increment);
			}
		}
	}

	private void GenerateSources(SourceProductionContext context, ImmutableArray<EndpointIncrement> increments)
	{
		if (increments.Length == 0)
		{
			return;
		}

		var builder = new EndpointSourceBuilder(increments);
		var source = builder.Build();
		context.AddSource(Sources.GeneratedNameHint("NuonEndpointExtensions"), source);
	}
}
