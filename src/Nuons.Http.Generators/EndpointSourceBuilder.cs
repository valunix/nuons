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

		foreach (var increment in increments)
		{
			var groupVarName = GenerateGroupVariableName(increment.HandlerType);

			// Emit the MapGroup call
			var groupLine = $@"var {groupVarName} = app.MapGroup(""{increment.RoutePrefix}"");";
			endpointLines.Append(Sources.NewLine);
			endpointLines.Append(Sources.Tab2);
			endpointLines.Append(groupLine);
			endpointLines.Append(Sources.NewLine);

			foreach (var config in increment.GroupConfigurationCalls)
			{
				endpointLines.Append(Sources.Tab2);
				endpointLines.Append(BuildConfigStatement(groupVarName, config));
				endpointLines.Append(Sources.NewLine);
			}

			foreach (var endpoint in increment.Endpoints)
			{
				var parameters = string.Empty;
				var parametersFull = string.Empty;
				if (endpoint.HandlerMethodParameters.Length > 0)
				{
					parameters = string.Join(ParameterSeparator, endpoint.HandlerMethodParameters.Select(parameter => parameter.Name));
					parametersFull = ParameterSeparator + string.Join(ParameterSeparator, endpoint.HandlerMethodParameters.Select(BuildParameterString));
				}
				var lambdaPrefix = endpoint.IsAsync ? "async " : "";
			var awaitPrefix = endpoint.IsAsync ? "await " : "";
			var endpointLine = @$"{groupVarName}.Map{endpoint.HttpMethod}(""{endpoint.Route}""{ParameterSeparator}{lambdaPrefix}({increment.HandlerType} handler{parametersFull}) => {awaitPrefix}handler.{endpoint.HandlerMethod}({parameters}))";

				endpointLines.Append(Sources.Tab2);
				endpointLines.Append(endpointLine);
				foreach (var config in endpoint.ConfigurationCalls)
				{
					endpointLines.Append(BuildConfigCall(config));
				}
				endpointLines.Append(";");
				endpointLines.Append(Sources.NewLine);
			}

			var serviceLine = $@"services.AddScoped<{increment.HandlerType}>();";

			serviceLines.Append(Sources.NewLine);
			serviceLines.Append(Sources.Tab2);
			serviceLines.Append(serviceLine);
		}

		var source = $@"using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Nuons.Http.Extensions;

internal static class NuonEndpointExtensions
{{
	public static void AddNuonHttpServices(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
	{{{serviceLines}
	}}

	public static void MapNuonEndpoints(this global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
	{{{endpointLines}
	}}
}}";

		return source;
	}

	private static string BuildConfigStatement(string target, EndpointConfigurationCall config)
	{
		return $"{target}{BuildConfigCall(config)};";
	}

	private static string BuildConfigCall(EndpointConfigurationCall config)
	{
		if (config.Arguments.IsEmpty)
		{
			return $".{config.Method}()";
		}
		else
		{
			var args = string.Join(", ", config.Arguments.Select(a => $"\"{a}\""));
			return $".{config.Method}({args})";
		}
	}

	private static string GenerateGroupVariableName(string handlerType)
	{
		// Extract just the class name (last part after any dots)
		var lastDot = handlerType.LastIndexOf('.');
		var className = lastDot >= 0 ? handlerType.Substring(lastDot + 1) : handlerType;

		// Convert to camelCase and add "Group" suffix
		// PascalCase -> camelCase by lowercasing first letter
		var camelCase = char.ToLowerInvariant(className[0]) + className.Substring(1);
		return camelCase + "Group";
	}

	private static string BuildParameterString(MethodParameter param)
	{
		var builder = new StringBuilder();
		if (param.BindingAttribute is { } binding)
		{
			if (binding.Name is not null)
			{
				builder.Append($"[global::{binding.AspNetAttribute}(Name = \"{binding.Name}\")] ");
			}
			else
			{
				builder.Append($"[global::{binding.AspNetAttribute}] ");
			}
		}
		builder.Append($"{param.Type} {param.Name}");
		return builder.ToString();
	}
}
