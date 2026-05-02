using System.Collections.Immutable;

namespace Nuons.Http.Generators;

internal record EndpointConfigurationCall(string Method, ImmutableArray<string> Arguments);

internal record EndpointIncrement(string HandlerType, string RoutePrefix, ImmutableArray<EndpointRegistration> Endpoints, ImmutableArray<EndpointConfigurationCall> GroupConfigurationCalls);

internal record EndpointRegistration(string HttpMethod, string Route, string HandlerMethod, ImmutableArray<MethodParameter> HandlerMethodParameters, ImmutableArray<EndpointConfigurationCall> ConfigurationCalls, bool IsAsync);

internal record ParameterBindingAttribute(string AspNetAttribute, string? Name);

internal record MethodParameter(string Type, string Name, ParameterBindingAttribute? BindingAttribute = null);
