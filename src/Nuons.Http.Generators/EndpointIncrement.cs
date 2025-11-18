using System.Collections.Immutable;

namespace Nuons.Http.Generators;

internal record EndpointIncrement(HandlerRegistration Handler, ImmutableArray<EndpointRegistration> Endpoints)
{
	//public static EndpointIncrement NewGetEndpoint(string route, string implementationType, string implementationMethod, ImmutableArray<MethodParameter> methodParameters)
	//	=> new EndpointIncrement(HttpMethods.Get, route, implementationType, implementationMethod, methodParameters);
}

internal record HandlerRegistration(string Type);

internal record EndpointRegistration(string HttpMethod, string Route, string HandlerMethod, ImmutableArray<MethodParameter> HandlerMethodParameters);

internal record MethodParameter(string Type, string Name);
