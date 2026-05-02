using Microsoft.CodeAnalysis;

namespace Nuons.Http.Generators;

internal record EndpointAttributes(
	INamedTypeSymbol Route,
	INamedTypeSymbol Get,
	INamedTypeSymbol Post,
	INamedTypeSymbol Put,
	INamedTypeSymbol Delete,
	INamedTypeSymbol Patch
)
{
	public IEnumerable<(INamedTypeSymbol Symbol, string HttpMethod)> HttpMethodAttributes()
	{
		yield return (Get, HttpMethods.Get);
		yield return (Post, HttpMethods.Post);
		yield return (Put, HttpMethods.Put);
		yield return (Delete, HttpMethods.Delete);
		yield return (Patch, HttpMethods.Patch);
	}

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

		var patchAttributeSymbol = compilation.GetTypeByMetadataName(KnownHttpTypes.PatchAttribute);
		if (patchAttributeSymbol is null)
		{
			return null;
		}

		return new EndpointAttributes(routeAttributeSymbol, getAttributeSymbol, postAttributeSymbol, putAttributeSymbol, deleteAttributeSymbol, patchAttributeSymbol);
	}
}
