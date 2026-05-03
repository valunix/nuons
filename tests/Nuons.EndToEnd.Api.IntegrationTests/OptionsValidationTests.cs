using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;

namespace Nuons.EndToEnd.Api.IntegrationTests;

public class OptionsValidationTests(WebApplicationFactory<Program> webApplicationFactory)
	: IClassFixture<WebApplicationFactory<Program>>
{
	[Fact]
	public async Task ValidatedOptionsEndpoint_WithValidConfiguration_ReturnsValue()
	{
		// Arrange
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.ValidatedOptions, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe("ValidatedName");
	}

	[Fact]
	public async Task LazyValidatedOptionsEndpoint_WithValidConfiguration_ReturnsValue()
	{
		// Arrange
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.LazyValidatedOptions, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe("3");
	}

	[Fact]
	public async Task LazyValidatedOptionsEndpoint_WithInvalidConfiguration_FailsWhenResolved()
	{
		// Arrange - Validate without ValidateOnStart: the host starts, validation only
		// fires when the options are resolved by the endpoint, surfacing as a 500.
		using var factory = new CustomConfigWebApplicationFactory(new Dictionary<string, string?>
		{
			["LazyValidatedOptions:Count"] = "999",
		});
		using var client = factory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.LazyValidatedOptions, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
	}

	[Fact]
	public void ValidateOnStart_WithInvalidConfiguration_FailsHostStartup()
	{
		// Arrange - ValidateOnStart forces validation while the host starts, so building the
		// client (which starts the server) throws rather than deferring to first resolution.
		using var factory = new CustomConfigWebApplicationFactory(new Dictionary<string, string?>
		{
			["ValidatedOptions:Count"] = "999",
		});

		// Act / Assert
		Should.Throw<OptionsValidationException>(() => factory.CreateClient());
	}
}
