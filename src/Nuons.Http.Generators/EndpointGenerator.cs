using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Nuons.Core.Generators;

namespace Nuons.Http.Generators;

[Generator]
internal partial class EndpointGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var endpointAttributesProvider = context.CompilationProvider
			.Select(EndpointAttributes.FromCompilation);

		var currentProjectProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			KnownHttpTypes.RouteAttribute,
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
		var routeAttribute = compilation.GetTypeByMetadataName(KnownHttpTypes.RouteAttribute);
		if (routeAttribute is null)
		{
			return [];
		}

		var nuonMarkerAttribute = compilation.GetTypeByMetadataName(KnownCoreTypes.AssemblyHasNuonsAttribute);
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

		var routePrefix = NormalizePrefix(route);

		var groupConfigBuilder = ImmutableArray.CreateBuilder<EndpointConfigurationCall>();
		foreach (var attr in symbol.GetAttributes())
		{
			ExtractSharedConfigCall(attr, groupConfigBuilder);
		}

		var endpointBuilder = ImmutableArray.CreateBuilder<EndpointRegistration>();

		foreach (var member in symbol.GetMembers().OfType<IMethodSymbol>())
		{
			if (TryCreateEndpoint(member, endpointAttributes, out var endpoint))
			{
				endpointBuilder.Add(endpoint!);
			}
		}

		var finalEndpoints = endpointBuilder.ToImmutable();
		if (finalEndpoints.Length == 0)
		{
			return null;
		}

		var increment = new EndpointIncrement(handlerType, routePrefix, finalEndpoints, groupConfigBuilder.ToImmutable());
		return increment;
	}

	private static bool TryCreateEndpoint(IMethodSymbol member, EndpointAttributes endpointAttributes, out EndpointRegistration? endpoint)
	{
		foreach (var (attributeSymbol, httpMethod) in endpointAttributes.HttpMethodAttributes())
		{
			var attribute = member.FirstOrDefaultAttribute(attributeSymbol);
			if (attribute is not null)
			{
				if (TryCreateEndpoint(member, attribute, httpMethod, out endpoint))
				{
					return true;
				}
			}
		}

		endpoint = null;
		return false;
	}

	private static bool TryCreateEndpoint(IMethodSymbol member, AttributeData attribute, string httpMethod, out EndpointRegistration? endpoint)
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
			var bindingAttribute = GetBindingAttribute(parameter);
			parameterBuilder.Add(new MethodParameter(typeName, name, bindingAttribute));
		}

		var configBuilder = ImmutableArray.CreateBuilder<EndpointConfigurationCall>();
		foreach (var attr in member.GetAttributes())
		{
			var fullName = attr.AttributeClass?.ToDisplayString();
			if (fullName == KnownHttpTypes.AllowAnonymousAttribute)
			{
				configBuilder.Add(new EndpointConfigurationCall("AllowAnonymous", ImmutableArray<string>.Empty));
			}
			else if (fullName == KnownHttpTypes.EndpointNameAttribute)
			{
				var name = attr.ConstructorArguments[0].Value as string;
				if (name is not null)
				{
					configBuilder.Add(new EndpointConfigurationCall("WithName", ImmutableArray.Create(name)));
				}
			}
			else if (fullName == KnownHttpTypes.EndpointDescriptionAttribute)
			{
				var desc = attr.ConstructorArguments[0].Value as string;
				if (desc is not null)
				{
					configBuilder.Add(new EndpointConfigurationCall("WithDescription", ImmutableArray.Create(desc)));
				}
			}
			else
			{
				ExtractSharedConfigCall(attr, configBuilder);
			}
		}

		var implementationMethod = member.Name;
		var subRoute = NormalizeSubRoute(route);
		var isAsync = IsAsyncReturnType(member);
		endpoint = new EndpointRegistration(httpMethod, subRoute, implementationMethod, parameterBuilder.ToImmutable(), configBuilder.ToImmutable(), isAsync);
		return true;
	}

	private static bool IsAsyncReturnType(IMethodSymbol method)
	{
		var returnType = method.ReturnType;
		var fullName = returnType.OriginalDefinition.ToDisplayString();

		return fullName is
			"System.Threading.Tasks.Task" or
			"System.Threading.Tasks.Task<TResult>" or
			"System.Threading.Tasks.ValueTask" or
			"System.Threading.Tasks.ValueTask<TResult>";
	}

	private static void ExtractSharedConfigCall(AttributeData attr, ImmutableArray<EndpointConfigurationCall>.Builder builder)
	{
		var fullName = attr.AttributeClass?.ToDisplayString();
		if (fullName == KnownHttpTypes.AuthorizeAttribute)
		{
			var policy = GetStringArg(attr);
			var args = policy is not null
				? ImmutableArray.Create(policy)
				: ImmutableArray<string>.Empty;
			builder.Add(new EndpointConfigurationCall("RequireAuthorization", args));
		}
		else if (fullName == KnownHttpTypes.TagsAttribute)
		{
			var tags = ImmutableArray.CreateBuilder<string>();
			foreach (var arg in attr.ConstructorArguments)
			{
				if (arg.Kind == TypedConstantKind.Array)
				{
					foreach (var item in arg.Values)
					{
						if (item.Value is string s)
						{
							tags.Add(s);
						}
					}
				}
				else if (arg.Value is string s)
				{
					tags.Add(s);
				}
			}
			builder.Add(new EndpointConfigurationCall("WithTags", tags.ToImmutable()));
		}
		else if (fullName == KnownHttpTypes.EnableRateLimitingAttribute)
		{
			var policy = GetStringArg(attr);
			if (policy is not null)
			{
				builder.Add(new EndpointConfigurationCall("RequireRateLimiting", ImmutableArray.Create(policy)));
			}
		}
		else if (fullName == KnownHttpTypes.RequireCorsAttribute)
		{
			var policy = GetStringArg(attr);
			if (policy is not null)
			{
				builder.Add(new EndpointConfigurationCall("RequireCors", ImmutableArray.Create(policy)));
			}
		}
		else if (fullName == KnownHttpTypes.DisableAntiforgeryAttribute)
		{
			builder.Add(new EndpointConfigurationCall("DisableAntiforgery", ImmutableArray<string>.Empty));
		}
	}

	private static ParameterBindingAttribute? GetBindingAttribute(IParameterSymbol parameter)
	{
		foreach (var attr in parameter.GetAttributes())
		{
			var fullName = attr.AttributeClass?.ToDisplayString();
			switch (fullName)
			{
				case KnownHttpTypes.FromHeaderAttribute:
					return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromHeaderAttribute", GetNameArg(attr));
				case KnownHttpTypes.FromQueryAttribute:
					return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromQueryAttribute", GetNameArg(attr));
				case KnownHttpTypes.FromBodyAttribute:
					return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromBodyAttribute", null);
				case KnownHttpTypes.FromRouteAttribute:
					return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromRouteAttribute", GetNameArg(attr));
				case KnownHttpTypes.FromFormAttribute:
					return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromFormAttribute", GetNameArg(attr));
				case KnownHttpTypes.FromServicesAttribute:
					return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.FromServicesAttribute", null);
				case KnownHttpTypes.AsParametersAttribute:
					return new ParameterBindingAttribute("Microsoft.AspNetCore.Http.AsParametersAttribute", null);
			}
		}
		return null;
	}

	private static string? GetNameArg(AttributeData attr)
	{
		if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string name && !string.IsNullOrEmpty(name))
		{
			return name;
		}
		else
		{
			return null;
		}
	}

	private static string? GetStringArg(AttributeData attr)
	{
		if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string s && !string.IsNullOrEmpty(s))
		{
			return s;
		}
		else
		{
			return null;
		}
	}

	private static string NormalizePrefix(string route)
	{
		// Ensure leading slash, no trailing slash
		var trimmed = route.Trim('/');
		return "/" + trimmed;
	}

	private static string NormalizeSubRoute(string route)
	{
		// Sub-routes are relative, so we just trim leading and trailing slashes
		// This allows them to be combined with group routes properly
		return route.Trim('/');
	}

	private static void GenerateSources(SourceProductionContext context, ImmutableArray<EndpointIncrement> increments)
	{
		var builder = new EndpointSourceBuilder(increments);
		var source = builder.Build();
		context.AddSource(Sources.GeneratedNameHint("NuonEndpointExtensions"), source);
	}
}
