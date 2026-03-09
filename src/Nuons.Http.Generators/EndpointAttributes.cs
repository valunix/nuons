using Microsoft.CodeAnalysis;

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
		var routeAttributeSymbol = compilation.GetTypeByMetadataName(KnownHttpTypes.RouteAttribute);
		if (routeAttributeSymbol is null)
		{
			return null;
		}

		var getAttributeSymbol = compilation.GetTypeByMetadataName(KnownHttpTypes.GetAttribute);
		if (getAttributeSymbol is null)
		{
			return null;
		}

		var postAttributeSymbol = compilation.GetTypeByMetadataName(KnownHttpTypes.PostAttribute);
		if (postAttributeSymbol is null)
		{
			return null;
		}

		var putAttributeSymbol = compilation.GetTypeByMetadataName(KnownHttpTypes.PutAttribute);
		if (putAttributeSymbol is null)
		{
			return null;
		}

		var deleteAttributeSymbol = compilation.GetTypeByMetadataName(KnownHttpTypes.DeleteAttribute);
		if (deleteAttributeSymbol is null)
		{
			return null;
		}

		return new EndpointAttributes(routeAttributeSymbol, getAttributeSymbol, postAttributeSymbol, putAttributeSymbol, deleteAttributeSymbol);
	}
}
