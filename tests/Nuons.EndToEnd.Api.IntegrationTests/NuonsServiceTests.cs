using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Nuons.EndToEnd.ScopedFeature.Infrastructure;
using Nuons.EndToEnd.ServiceFeature.Infrastructure;
using Nuons.EndToEnd.SingletonFeature.Infrastructure;
using Nuons.EndToEnd.TransientFeature.Infrastructure;

namespace Nuons.EndToEnd.Api.IntegrationTests;

public class NuonsServiceTests(WebApplicationFactory<Program> webApplicationFactory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetSingletonEndpoint_ReturnsSingletonValue_WhenCalled()
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
    public async Task GetTransientEndpoint_ReturnsTransientValue_WhenCalled()
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
    public async Task GetScopedEndpoint_ReturnsScopedValue_WhenCalled()
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
    public async Task GetServiceAttributeEndpoint_ReturnsServiceAttributeValue_WhenCalled()
    {
        // Arrange 
        using var client = webApplicationFactory.CreateClient();
        
        // Act
        using var response = await client.GetAsync(Routes.Service, TestContext.Current.CancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.ShouldBe(ServiceAttributeService.Value);
    }
}