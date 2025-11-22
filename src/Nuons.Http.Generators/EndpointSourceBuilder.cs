using System.Collections.Immutable;
using System.Text;
using Nuons.Core.Generators;

namespace Nuons.Http.Generators;

internal class EndpointSourceBuilder(ImmutableArray<EndpointIncrement> increments)
{
	private const string ParameterSeparator = ", ";

	public string Build()
	{
		var endpointLines = new StringBuilder();
		var serviceLines = new StringBuilder();

		foreach(var increment in increments)
		{
			foreach(var endpoint in increment.Endpoints)
			{
				var parameters = string.Empty;
				var parametersFull = string.Empty;
				if (endpoint.HandlerMethodParameters.Length > 0)
				{
					parameters = string.Join(ParameterSeparator, endpoint.HandlerMethodParameters.Select(parameter => parameter.Name));
					parametersFull = ParameterSeparator + string.Join(ParameterSeparator, endpoint.HandlerMethodParameters.Select(parameter => $"{parameter.Type} {parameter.Name}"));
				}
				var endpointLine = @$"app.Map{endpoint.HttpMethod}(""{endpoint.Route}"", ({increment.Handler.Type} handler{parametersFull}) => handler.{endpoint.HandlerMethod}({parameters}));";

				endpointLines.Append(Sources.NewLine);
				endpointLines.Append(Sources.Tab2);
				endpointLines.Append(endpointLine);
			}

			var serviceLine = $@"services.AddScoped<{increment.Handler.Type}>();";

			serviceLines.Append(Sources.NewLine);
			serviceLines.Append(Sources.Tab2);
			serviceLines.Append(serviceLine);
		}

		var source = $@"using Microsoft.Extensions.DependencyInjection;

internal static class NuonEndpointExtensions
{{
	public static void AddNuonServices(this IServiceCollection services)
	{{{serviceLines}
	}}

	public static void MapNuonEndpoints(this WebApplication app)
	{{{endpointLines}
	}}
}}";

		return source;
	}
}
