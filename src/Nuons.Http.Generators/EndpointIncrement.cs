namespace Nuons.Http.Generators;

internal record EndpointIncrement(
	string HttpMethod,
	string Route,
	string ImplementationType,
	string ImplementationMethod);
