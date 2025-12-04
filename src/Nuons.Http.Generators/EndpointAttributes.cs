using Microsoft.CodeAnalysis;
using Nuons.Http.Abstractions;

namespace Nuons.Http.Generators;

internal record EndpointAttributes(
	INamedTypeSymbol Route,
	INamedTypeSymbol Get,
	INamedTypeSymbol Post,
	INamedTypeSymbol Put,
	INamedTypeSymbol Delete
)
{
	public static EndpointAttributes? FromCompilation(Compilation compilation, CancellationToken cancellationToken)
	{
		var routeAttributeSymbol = compilation.GetTypeByMetadataName(typeof(RouteAttribute).FullName);
		if (routeAttributeSymbol is null)
		{
			return null;
		}

		var getAttributeSymbol = compilation.GetTypeByMetadataName(typeof(GetAttribute).FullName);
		if (getAttributeSymbol is null)
		{
			return null;
		}

		var postAttributeSymbol = compilation.GetTypeByMetadataName(typeof(PostAttribute).FullName);
		if (postAttributeSymbol is null)
		{
			return null;
		}

		var putAttributeSymbol = compilation.GetTypeByMetadataName(typeof(PutAttribute).FullName);
		if (putAttributeSymbol is null)
		{
			return null;
		}

		var deleteAttributeSymbol = compilation.GetTypeByMetadataName(typeof(DeleteAttribute).FullName);
		if (deleteAttributeSymbol is null)
		{
			return null;
		}

		return new EndpointAttributes(routeAttributeSymbol, getAttributeSymbol, postAttributeSymbol, putAttributeSymbol, deleteAttributeSymbol);
	}
}
