using System.Net;

using Microsoft.Extensions.Logging;
using Moq;
using Resiliency.Patterns.Labs.Api.Services;
using Resiliency.Patterns.Labs.Api.Services.Interfaces;

namespace Resiliency.Patterns.Labs.Tests.Services;

public class HttpBinServiceTest
{
    private readonly IHttpBinService _service;

    private readonly Mock<ILogger<HttpBinService>> _mockHttpBinService;

    public HttpBinServiceTest()
    {
        Mock<HttpClient> mockHttpClient = new();
        _mockHttpBinService = new Mock<ILogger<HttpBinService>>();
        
        _service = new HttpBinService(httpClient: mockHttpClient.Object, logger: _mockHttpBinService.Object);
    }

    [Fact]
    public void Get_Returns_Given_StatusCode()
    {
        var response = _service.Get(200);
        
        Equal(200, response.Result);
        
        _mockHttpBinService.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => string.Equals("True", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

public interface IHttpClientWrapper
{
    Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken);
}