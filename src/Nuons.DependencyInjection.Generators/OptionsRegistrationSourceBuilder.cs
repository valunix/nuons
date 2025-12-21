using System.Text;
using Nuons.Core.Generators;

namespace Nuons.DependencyInjection.Generators;

internal class OptionsRegistrationSourceBuilder
{
	private readonly string className;
	private readonly List<string> registrations = [];

	public OptionsRegistrationSourceBuilder(string className)
	{
		this.className = className;
	}

	public void WithOptions(OptionsRegistration registration)
	{
		var source = $"{Sources.Tab2}services.Configure<{registration.ClassName}>(configuration.GetSection(\"{registration.SectionKey}\"));";
		registrations.Add(source);
	}

	public string Build()
	{
		var builder = new StringBuilder();

		registrations.ForEach(registration =>
		{
			builder.AppendLine();
			builder.Append(registration);
		});

		var source = $@"using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nuons.DependencyInjection.Extensions;

public static class {className}
{{
	public static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
	{{{builder}
	}}
}}";

		return source;
	}
}
