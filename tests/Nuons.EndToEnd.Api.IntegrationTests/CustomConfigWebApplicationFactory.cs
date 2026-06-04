using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Nuons.EndToEnd.Api.IntegrationTests;

/// <summary>
/// Web app factory that allows to include additional custom configuration for custom tests.
/// </summary>
/// <param name="customConfiguration"></param>
internal sealed class CustomConfigWebApplicationFactory(IReadOnlyDictionary<string, string?> customConfiguration)
	: WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((_, configuration) =>
			configuration.AddInMemoryCollection(customConfiguration));
	}
}
