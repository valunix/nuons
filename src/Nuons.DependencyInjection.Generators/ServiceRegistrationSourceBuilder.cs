using System.Text;
using Nuons.Core.Generators;

namespace Nuons.DependencyInjection.Generators;

internal class ServiceRegistrationSourceBuilder
{
	private readonly string className;
	private readonly List<string> registrations = [];

	public ServiceRegistrationSourceBuilder(string className)
	{
		this.className = className;
	}

	public void WithRegistration(ServiceRegistration registration) =>
		WithRegistration(registration.ServiceType, registration.ImplementingType, registration.Lifetime);

	private void WithRegistration(string serviceType, string implementingType, Lifetime lifetime)
	{
		var registration = $"{Sources.Tab2}services.Add{lifetime.ToMethodName()}<{serviceType}, {implementingType}>();";
		registrations.Add(registration);
	}

	public string Build()
	{
		var registrationsBuilder = new StringBuilder();
		registrations.ForEach(registration =>
		{
			registrationsBuilder.AppendLine();
			registrationsBuilder.Append(registration);
		});

		var source = $@"using Microsoft.Extensions.DependencyInjection;

namespace Nuons.DependencyInjection.Extensions;

public static class {className}
{{
	public static void AddServices(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
	{{{registrationsBuilder}
	}}
}}";
		return source;
	}
}
