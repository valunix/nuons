using System.Collections.Immutable;

namespace Nuons.Http.Generators;

internal record EndpointIncrement(string HandlerType, ImmutableArray<EndpointRegistration> Endpoints);

internal record EndpointRegistration(string HttpMethod, string Route, string HandlerMethod, ImmutableArray<MethodParameter> HandlerMethodParameters);

internal record MethodParameter(string Type, string Name);
