using System.Text;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators.Registration;

internal class CombinedRegistrationSourceBuilder
{
	private readonly CombinedRegistrationsIncrement increment;

	public CombinedRegistrationSourceBuilder(CombinedRegistrationsIncrement increment)
	{
		this.increment = increment;
	}

	public string Build()
	{
		var builder = new StringBuilder();
		if (increment.Service)
		{
			var registration = $"{Sources.GetServiceRegistrationClassName(increment.AssemblyName)}.RegisterServices(services);";
			Append(builder, registration);
		}

		if (increment.Singleton)
		{
			var registration = $"{Sources.GetLifetimeRegistrationClassName(increment.AssemblyName, Lifetime.Singleton)}.RegisterServices(services);";
			Append(builder, registration);
		}

		if (increment.Scoped)
		{
			var registration = $"{Sources.GetLifetimeRegistrationClassName(increment.AssemblyName, Lifetime.Scoped)}.RegisterServices(services);";
			Append(builder, registration);
		}

		if (increment.Transient)
		{
			var registration = $"{Sources.GetLifetimeRegistrationClassName(increment.AssemblyName, Lifetime.Transient)}.RegisterServices(services);";
			Append(builder, registration);
		}

		if (increment.Options)
		{
			var registration = $"{Sources.GetOptionsRegistrationClassName(increment.AssemblyName)}.ConfigureOptions(services, configuration);";
			Append(builder, registration);
		}

		var source = $@"using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nuons.DependencyInjection.Extensions;

public static class {Sources.GetCombinedRegistrationClassName(increment.AssemblyName)}
{{
	public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
	{{{builder}
	}}
}}";

		return source;
	}

	private void Append(StringBuilder builder, string registration)
	{
		builder.AppendLine();
		builder.Append(Sources.Tab2);
		builder.Append(registration);
	}
}
