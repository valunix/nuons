using System.Collections.Immutable;
using System.Text;
using Nuons.Core.Generators;

namespace Nuons.DependencyInjection.Generators.Startup;

internal class CombinedServiceRegistrationSourceBuilder
{
	private readonly string className;

	public CombinedServiceRegistrationSourceBuilder(string className)
	{
		this.className = className;
	}

	public string Build(ImmutableArray<CombinedServiceRegistrationIncrement> registrations)
	{
		var registrationsBuilder = new StringBuilder();
		registrations.ToList().ForEach(registration =>
		{
			registrationsBuilder.AppendLine();

			var serviceClassName = DependancyInjectionSources.GetServiceRegistrationClassName(registration.AssemblyName);
			var serviceLine = $"{Sources.Tab2}{serviceClassName}.AddServices(services);";
			registrationsBuilder.Append(serviceLine);

			registrationsBuilder.AppendLine();

			var optionsClassName = DependancyInjectionSources.GetOptionsRegistrationClassName(registration.AssemblyName);
			var optionsLine = $"{Sources.Tab2}{optionsClassName}.ConfigureOptions(services, configuration);";
			registrationsBuilder.Append(optionsLine);
		});

		var source = $@"using Microsoft.Extensions.DependencyInjection;

namespace Nuons.DependencyInjection.Extensions;

public static class {className}
{{
	public static void AddNuonDependancyInjectionServices(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services, global::Microsoft.Extensions.Configuration.IConfiguration configuration)
	{{{registrationsBuilder}
	}}
}}";
		return source;
	}
}
