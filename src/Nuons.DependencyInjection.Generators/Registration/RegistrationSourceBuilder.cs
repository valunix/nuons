using System.Text;

namespace Nuons.DependencyInjection.Generators.Registration;

internal class RegistrationSourceBuilder
{
	private readonly string className;
	private readonly List<string> registrations = [];

	public RegistrationSourceBuilder(string className)
	{
		this.className = className;
	}

	public void WithRegistration(ServiceRegistration registration, Lifetime lifetime) =>
		WithRegistration(registration.ServiceType, registration.ImplementingType, lifetime);

	public void WithRegistration(ServiceLifetimeRegistration registration) =>
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
	public static void RegisterServices(IServiceCollection services)
	{{{registrationsBuilder}
	}}
}}";
		return source;
	}
}
