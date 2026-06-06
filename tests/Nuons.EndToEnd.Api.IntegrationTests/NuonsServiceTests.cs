using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Nuons.EndToEnd.ScopedFeature.Infrastructure;
using Nuons.EndToEnd.SingletonFeature.Infrastructure;
using Nuons.EndToEnd.TransientFeature.Infrastructure;

namespace Nuons.EndToEnd.Api.IntegrationTests;

public class NuonsServiceTests(WebApplicationFactory<Program> webApplicationFactory)
	: IClassFixture<WebApplicationFactory<Program>>
{
	[Fact]
	public async Task SingletonEndpoint_ReturnsCorrectValue()
	{
		// Arrange 
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.Singleton, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe(SingletonService.Value);
	}

	[Fact]
	public async Task SingletonGenericEndpoint_ReturnsCorrectValue()
	{
		// Arrange 
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.SingletonGeneric, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe(SingletonGenericService.Value);
	}

	[Fact]
	public async Task GetTransientEndpoint_ReturnsCorrectValue()
	{
		// Arrange 
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.Transient, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe(TransientService.Value);
	}

	[Fact]
	public async Task GetTransientGenericEndpoint_ReturnsCorrectValue()
	{
		// Arrange 
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.TransientGeneric, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe(TransientGenericService.Value);
	}

	[Fact]
	public async Task GetScopedEndpoint_ReturnsCorrectValue()
	{
		// Arrange 
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.Scoped, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe(ScopedService.Value);
	}

	[Fact]
	public async Task GetScopedGenericEndpoint_ReturnsCorrectValue()
	{
		// Arrange 
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.ScopedGeneric, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe(ScopedGenericService.Value);
	}

	[Fact]
	public async Task GetEmptyConstructorEndpoint_ReturnsCorrectValue()
	{
		// Arrange
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.EmptyConstructor, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe("EmptyConstructorValue");
	}

	[Fact]
	public async Task GetGenericInjectConstructorEndpoint_ReturnsCorrectValue()
	{
		// Arrange
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.GenericInjectConstructor, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe($"{nameof(String)}:{SingletonService.Value}");
	}

	[Fact]
	public async Task GetGlobalNamespaceEndpoint_ReturnsCorrectValue()
	{
		// Arrange
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.GlobalNamespaceConstructor, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe(GlobalNamespaceService.Value);
	}

	[Fact]
	public async Task GetComplexEndpoint_ReturnsCorrectValue()
	{
		// Arrange 
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.Complex, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe(SingletonService.Value);
	}

	[Fact]
	public async Task GetComplexOptionsEndpoint_ReturnsCorrectValue()
	{
		// Arrange 
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync(Routes.ComplexOptions, TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe("OptionsValue");
	}

	// TODO
	// separate http tests?
	// update to use constants
	[Fact]
	public async Task GetHttpEndpoint_ReturnsCorrectValue()
	{
		// Arrange 
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync("http-handler/get", TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe("get");
	}

	[Fact]
	public async Task GetNuonHttpEndpoint_ReturnsCorrectValue()
	{
		// Arrange 
		using var client = webApplicationFactory.CreateClient();

		// Act
		using var response = await client.GetAsync("nuon-http-handler/get", TestContext.Current.CancellationToken);

		// Assert
		response.StatusCode.ShouldBe(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		content.ShouldBe("get");
	}
}
