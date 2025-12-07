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
			var line = $"{Sources.Tab2}{GetServiceRegistrationClassName(registration.AssemblyName)}.AddServices(services);";
			registrationsBuilder.Append(line);
		});

		var source = $@"using Microsoft.Extensions.DependencyInjection;

namespace Nuons.DependencyInjection.Extensions;

public static class {className}
{{
	public static void AddNuonDependancyInjectionServices(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
	{{{registrationsBuilder}
	}}
}}";
		return source;
	}

	public static string GetServiceRegistrationClassName(string assemblyName) =>
		$"ServiceRegistration{Sources.TrimForClassName(assemblyName)}";
}
