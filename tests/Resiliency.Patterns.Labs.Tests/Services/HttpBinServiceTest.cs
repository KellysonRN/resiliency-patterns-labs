using Microsoft.Extensions.Logging;
using Moq;
using Resiliency.Patterns.Labs.Api.Services;
using Resiliency.Patterns.Labs.Api.Services.Interfaces;

namespace Resiliency.Patterns.Labs.Tests.Services;

public class HttpBinServiceTest
{
    private readonly IHttpBinService _service;

    public HttpBinServiceTest()
    {
        Mock<ILogger<HttpBinService>> mockHttpBinService = new();
        var client = new HttpClient();
        
        _service = new HttpBinService(httpClient: client, logger: mockHttpBinService.Object);
    }

    [Fact]
    public void Get_Returns_Given_StatusCode()
    {
        var response = _service.Get(200);
        
        Equal(200, response.Result);
    }
}