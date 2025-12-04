using System.Text;
using Nuons.Core.Generators;
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

		if (increment.Singleton)
		{
			var registration = $"{DependancyInjectionSources.GetLifetimeRegistrationClassName(increment.AssemblyName, Lifetime.Singleton)}.RegisterServices(services);";
			Append(builder, registration);
		}

		if (increment.Scoped)
		{
			var registration = $"{DependancyInjectionSources.GetLifetimeRegistrationClassName(increment.AssemblyName, Lifetime.Scoped)}.RegisterServices(services);";
			Append(builder, registration);
		}

		if (increment.Transient)
		{
			var registration = $"{DependancyInjectionSources.GetLifetimeRegistrationClassName(increment.AssemblyName, Lifetime.Transient)}.RegisterServices(services);";
			Append(builder, registration);
		}

		if (increment.Options)
		{
			var registration = $"{DependancyInjectionSources.GetOptionsRegistrationClassName(increment.AssemblyName)}.ConfigureOptions(services, configuration);";
			Append(builder, registration);
		}

		var source = $@"using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nuons.DependencyInjection.Extensions;

public static class {DependancyInjectionSources.GetCombinedRegistrationClassName(increment.AssemblyName)}
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
