using System.Collections.Immutable;

namespace Nuons.Http.Generators;

internal class EndpointSourceBuilder(ImmutableArray<EndpointIncrement> increments)
{
	public string Build()
	{

		var inc = increments[0];

		var endpointLine = @$"app.Map{inc.HttpMethod}(""{inc.Route}"", ({inc.ImplementationType} handler) => handler.{inc.ImplementationMethod}());";

		var serviceLine = $@"services.AddScoped<{inc.ImplementationType}>();";

		var source = $@"using Microsoft.Extensions.DependencyInjection;

internal static class NuonEndpointExtensions
{{
	
	public static void AddNuonServices(this IServiceCollection services)
	{{
		{serviceLine}
	}}

	public static void MapNuonEndpoints(this WebApplication app)
	{{
		{endpointLine}
	}}
}}";

		return source;
	}
}
