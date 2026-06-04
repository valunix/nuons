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
		var registrationSource = new StringBuilder();
		registrationSource.Append($"{Sources.Tab2}services.AddOptions<{registration.ClassName}>()");
		registrationSource.Append($"{Sources.NewLine}{Sources.Tab3}.Bind(configuration.GetSection(\"{registration.SectionKey}\"))");
		if (registration.Validate)
		{
			registrationSource.Append($"{Sources.NewLine}{Sources.Tab3}.ValidateDataAnnotations()");
		}
		if (registration.ValidateOnStart)
		{
			registrationSource.Append($"{Sources.NewLine}{Sources.Tab3}.ValidateOnStart()");
		}
		registrationSource.Append(";");
		registrations.Add(registrationSource.ToString());
	}

	public string Build()
	{
		var builder = new StringBuilder();

		registrations.ForEach(registration =>
		{
			builder.AppendLine();
			builder.Append(registration);
		});

		var source = $@"{Sources.GeneratedFileHeader}
using Microsoft.Extensions.Configuration;
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
