using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Nuons.Core.Abstractions;
using Nuons.Core.Generators;
using Nuons.Http.Abstractions;

namespace Nuons.Http.Generators;

[Generator]
internal partial class EndpointGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var endpointAttributesProvider = context.CompilationProvider
			.Select(EndpointAttributes.FromCompilation);

		var currentProjectProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(RouteAttribute).FullName,
			Syntax.IsClassNode,
			ExtractSymbol
		)
			.WhereNotNull()
			.Collect();

		var referencesProvider = context.CompilationProvider
			.SelectMany(ExtractSymbolsFromReferences)
			.Collect();

		var mergedProvider = currentProjectProvider
			.Combine(referencesProvider)
			.Select(static (combined, ct) => combined.Left.AddRange(combined.Right))
			.Combine(endpointAttributesProvider)
			.Select(static (combined, ct) => new EndpointSubIncrement(combined.Left, combined.Right))
			.Select(ExtractIncrements);

		context.RegisterSourceOutput(mergedProvider, GenerateSources);
	}

	private static INamedTypeSymbol? ExtractSymbol(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
	{
		return context.TargetSymbol as INamedTypeSymbol;
	}

	private static ImmutableArray<INamedTypeSymbol> ExtractSymbolsFromReferences(Compilation compilation, CancellationToken cancellationToken)
	{
		var routeAttribute = compilation.GetTypeByMetadataName(typeof(RouteAttribute).FullName!);
		if (routeAttribute is null)
		{
			return [];
		}

		var nuonMarkerAttribute = compilation.GetTypeByMetadataName(typeof(AssemblyHasNuonsAttribute).FullName!);
		if (nuonMarkerAttribute is null)
		{
			return [];
		}

		var results = compilation.SourceModule.ReferencedAssemblySymbols
			.Where(a => a.HasAttribute(nuonMarkerAttribute))
			.SelectMany(a => a.GetAllTypes())
			.Where(type => type.HasAttribute(routeAttribute));

		return [.. results];
	}

	private static ImmutableArray<EndpointIncrement> ExtractIncrements(EndpointSubIncrement subIncrement, CancellationToken token)
	{
		var increments = subIncrement.Symbols
			.Select(symbol => ExtractIncrement(symbol, subIncrement.Attributes, token))
			.OfType<EndpointIncrement>();
		return [.. increments];
	}

	private static EndpointIncrement? ExtractIncrement(INamedTypeSymbol symbol, EndpointAttributes? endpointAttributes, CancellationToken cancellationToken)
	{
		if (endpointAttributes is null)
		{
			return null;
		}

		var attribute = symbol.FirstOrDefaultAttribute(endpointAttributes.Route);
		if (attribute is null)
		{
			return null;
		}

		var constructorArguments = attribute.ConstructorArguments;
		if (constructorArguments.Length is not 1)
		{
			return null;
		}

		if (constructorArguments[0].Value is not string route)
		{
			return null;
		}

		var handlerType = symbol.ToFullTypeName();
		if (string.IsNullOrEmpty(handlerType))
		{
			return null;
		}

		var routeBuilder = new RouteBuilder(route);

		var endpointBuilder = ImmutableArray.CreateBuilder<EndpointRegistration>();

		foreach (var member in symbol.GetMembers().OfType<IMethodSymbol>())
		{
			if (TryCreateEndpoint(member, routeBuilder, endpointAttributes, out var endpoint))
			{
				endpointBuilder.Add(endpoint!);
			}
		}

		var finalEndpoints = endpointBuilder.ToImmutable();
		if (finalEndpoints.Length == 0)
		{
			return null;
		}

		var increment = new EndpointIncrement(handlerType, finalEndpoints);
		return increment;
	}

	// TODO foreach on endpoint spec instead of 4 IFs
	private static bool TryCreateEndpoint(IMethodSymbol member, RouteBuilder routeBuilder, EndpointAttributes endpointAttributes, out EndpointRegistration? endpoint)
	{
		var getAttribute = member.FirstOrDefaultAttribute(endpointAttributes.Get);
		if (getAttribute is not null)
		{
			if (TryCreateEndpoint(member, routeBuilder, getAttribute, HttpMethods.Get, out endpoint))
			{
				return true;
			}
		}

		var postAttribute = member.FirstOrDefaultAttribute(endpointAttributes.Post);
		if (postAttribute is not null)
		{
			if (TryCreateEndpoint(member, routeBuilder, postAttribute, HttpMethods.Post, out endpoint))
			{
				return true;
			}
		}

		var putAttribute = member.FirstOrDefaultAttribute(endpointAttributes.Put);
		if (putAttribute is not null)
		{
			if (TryCreateEndpoint(member, routeBuilder, putAttribute, HttpMethods.Put, out endpoint))
			{
				return true;
			}
		}

		var deleteAttribute = member.FirstOrDefaultAttribute(endpointAttributes.Delete);
		if (deleteAttribute is not null)
		{
			if (TryCreateEndpoint(member, routeBuilder, deleteAttribute, HttpMethods.Delete, out endpoint))
			{
				return true;
			}
		}

		endpoint = null;
		return false;
	}

	private static bool TryCreateEndpoint(IMethodSymbol member, RouteBuilder routeBuilder, AttributeData attribute, string httpMethod, out EndpointRegistration? endpoint)
	{
		endpoint = null;

		var getConstructorArguments = attribute.ConstructorArguments;
		if (getConstructorArguments.Length is not 1)
		{
			return false;
		}

		if (getConstructorArguments[0].Value is not string route)
		{
			return false;
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
		endpoint = new EndpointRegistration(httpMethod, fullRoute, implementationMethod, parameterBuilder.ToImmutable());
		return true;
	}

	private static void GenerateSources(SourceProductionContext context, ImmutableArray<EndpointIncrement> increments)
	{
		var builder = new EndpointSourceBuilder(increments);
		var source = builder.Build();
		context.AddSource(Sources.GeneratedNameHint("NuonEndpointExtensions"), source);
	}
}
