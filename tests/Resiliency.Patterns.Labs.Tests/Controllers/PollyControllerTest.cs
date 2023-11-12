using Microsoft.AspNetCore.Mvc.Testing;
using Resiliency.Patterns.Labs.Api.Models;

namespace Resiliency.Patterns.Labs.Tests.Controllers;

/// <summary>
/// "IClassFixture<WebApplicationFactory<Program>>" é usado para configurar um ambiente de teste que inclui uma instância de 
/// "WebApplicationFactory" para simular uma aplicação web ASP.NET Core e compartilhá-la entre as classes de teste em uma determinada 
/// classe de teste. Isso é útil para evitar a criação repetida da instância da fábrica de aplicativos web para cada teste, economizando tempo e recursos.
/// </summary>
public class PollyControllerTest  : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PollyControllerTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_ReturnsJsonResult_WithListOfEpisodes()
    {
        var client = _factory.CreateClient();

        var response = await client.DoGetAsync<PollyDto>("/api/polly/200");

        NotNull(response);
        Equal("World", response?.Hello);
        Equal(200, response?.Status);
    }
    
    [Fact]
    public async Task Get_ReturnsError500_InvalidPolicy()
    {
        var client = _factory.CreateClient();

        await ThrowsAsync<HttpRequestException>(() => client.DoGetAsync<PollyDto>("/api/polly/200?type=popcorn"));
    }
}
