using System;
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

		var postAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(PostAttribute).FullName);
		if (postAttributeSymbol is null)
		{
			return null;
		}

		var endpointContext = new EndpointGeneratorContext(getAttributeSymbol, postAttributeSymbol);
		return ExtractIncrement(symbol, endpointContext, cancellationToken);
	}

	private static EndpointIncrement? ExtractIncrement(INamedTypeSymbol symbol, EndpointGeneratorContext endpointContext, CancellationToken cancellationToken)
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

		var routeBuilder = new RouteBuilder(route);

		var implementationType = symbol.ToFullTypeName();
		var handler = new HandlerRegistration(implementationType);

		var endpointBuilder = ImmutableArray.CreateBuilder<EndpointRegistration>();

		var getMembers = symbol.GetMembers()
			.OfType<IMethodSymbol>()
			.Where(method => method.HasAttribute(endpointContext.GetAttributeSymbol))
			.ToList();
		if (getMembers.Count > 0)
		{
			AddEndpoints<GetAttribute>(endpointBuilder, getMembers, HttpMethods.Get, routeBuilder);
		}

		var postMembers = symbol.GetMembers()
			.OfType<IMethodSymbol>()
			.Where(method => method.HasAttribute(endpointContext.PostAttributeSymbol))
			.ToList();
		if (postMembers.Count > 0)
		{
			AddEndpoints<PostAttribute>(endpointBuilder, postMembers, HttpMethods.Post, routeBuilder);
		}

		var finalEndpoints = endpointBuilder.ToImmutable();
		if (finalEndpoints.Length == 0)
		{
			return null;
		}

		var increment = new EndpointIncrement(handler, finalEndpoints);
		return increment;
	}

	private static void AddEndpoints<TAttribute>(ImmutableArray<EndpointRegistration>.Builder endpointBuilder, List<IMethodSymbol> members, string httpMethod, RouteBuilder routeBuilder)
		where TAttribute : Attribute
	{
		foreach (var member in members)
		{
			var attribute = member.FirstOrDefaultAttribute<TAttribute>();
			if (attribute is null)
			{
				continue;
			}

			var getConstructorArguments = attribute.ConstructorArguments;
			if (getConstructorArguments.Length is not 1)
			{
				continue;
			}

			if (getConstructorArguments[0].Value is not string route)
			{
				continue;
			}

			var parameters = member.Parameters;
			var parameterBuilder = ImmutableArray.CreateBuilder<MethodParameter>();
			foreach (var parameter in parameters)
			{
				var typeName = parameter.Type.ToFullTypeName();
				var name = parameter.Name;
				parameterBuilder.Add(new MethodParameter(typeName, name));
			}

			var implementationMethod = member.Name;
			var fullRoute = routeBuilder.Build(route);
			var endpoint = new EndpointRegistration(httpMethod, fullRoute, implementationMethod, parameterBuilder.ToImmutable());
			endpointBuilder.Add(endpoint);
		}
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

		var postAttributeSymbol = compilation.GetTypeByMetadataName(typeof(PostAttribute).FullName);
		if (postAttributeSymbol is null)
		{
			return [];
		}

		var endpointContext = new EndpointGeneratorContext(getAttributeSymbol, postAttributeSymbol);

		foreach (var assemblySymbol in compilation.SourceModule.ReferencedAssemblySymbols)
		{
			if (!assemblySymbol.HasAttribute(nuonMarkerAttribute))
			{
				continue;
			}

			CollectAssemblyTypes(assemblySymbol, routeAttribute, endpointContext, results, cancellationToken);
		}

		return results.ToImmutable();
	}

	private static void CollectAssemblyTypes(IAssemblySymbol assemblySymbol, INamedTypeSymbol routeAttribute, EndpointGeneratorContext endpointContext, ImmutableArray<EndpointIncrement>.Builder sink, CancellationToken cancellationToken)
	{
		foreach (var type in assemblySymbol.GetAllTypes())
		{
			if (!type.HasAttribute(routeAttribute))
			{
				continue;
			}

			var increment = ExtractIncrement(type, endpointContext, cancellationToken);
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
