using Microsoft.AspNetCore.Mvc.Testing;

using Resiliency.Patterns.Labs.Api.Models;

namespace Resiliency.Patterns.Labs.Tests.Controllers;

public class PollyControllerTest : IClassFixture<WebApplicationFactory<Program>>
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
        Equal("World", response.Hello);
        Equal(200, response.Status);
    }

    [Fact]
    public async Task Get_ReturnsError500_InvalidPolicy()
    {
        var client = _factory.CreateClient();

        await ThrowsAsync<HttpRequestException>(() => client.DoGetAsync<PollyDto>("/api/polly/200?type=popcorn"));
    }
}