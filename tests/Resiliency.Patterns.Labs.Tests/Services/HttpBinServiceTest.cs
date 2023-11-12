using System.Net;

using Microsoft.Extensions.Logging;
using Moq;

using Resiliency.Patterns.Labs.Api.Configuration;
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
        Mock<ClientPolicy> mockClientPolicy = new();
        _mockHttpBinService = new Mock<ILogger<HttpBinService>>();

        _service = new HttpBinService(httpClient: mockHttpClient.Object, logger: _mockHttpBinService.Object,
            clientPolicy: mockClientPolicy.Object);
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
                It.Is<It.IsAnyType>((o, t) =>
                    string.Equals("True", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TestGetWithRetryPolicy_Success()
    {
        var result = await _service.GetWithRetryPolicy(500, 200);

        _mockHttpBinService.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("[Retry] HttpBinService returned a Success", StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.Equal(200, result);
    }
    
    [Fact]
    public async Task TestGetWithRetryPolicy_Fail()
    {
        var result = await _service.GetWithRetryPolicy(500);

        _mockHttpBinService.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("[Retry] HttpBinService returned a FAILURE", StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.Equal(500, result);
    }
}