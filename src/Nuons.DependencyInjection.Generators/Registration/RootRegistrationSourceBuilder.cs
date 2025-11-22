using System.Text;
using Nuons.Core.Generators;

namespace Nuons.DependencyInjection.Generators.Registration;

internal class RootRegistrationSourceBuilder
{
	private readonly RootRegistrationIncrement increment;

	public RootRegistrationSourceBuilder(RootRegistrationIncrement increment)
	{
		this.increment = increment;
	}

	public string Build()
	{
		var builder = new StringBuilder();
		increment.Registrations
			.Select(registration => $"{registration}.RegisterServices(services, configuration);")
			.ToList()
			.ForEach(registration => Append(builder, registration));

		var source = $@"using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuons.DependencyInjection.Extensions;

namespace {increment.namespaceName};

public partial class {increment.ClassName}
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
