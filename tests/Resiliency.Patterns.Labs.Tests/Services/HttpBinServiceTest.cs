using Microsoft.Extensions.Options;

using Moq;

using Resiliency.Patterns.Labs.Api.Configuration;
using Resiliency.Patterns.Labs.Api.Services;
using Resiliency.Patterns.Labs.Api.Services.Interfaces;

namespace Resiliency.Patterns.Labs.Tests.Services;

public class HttpBinServiceTest
{
    private readonly IHttpBinService _service;

    public HttpBinServiceTest()
    {
        Mock<HttpClient> mockHttpClient = new();
        Mock<ClientPolicy> mockClientPolicy = new();
        Mock<IOptions<HttpBinSettings>> mockSettings = new();

        _service = new HttpBinService(httpClient: mockHttpClient.Object, clientPolicy: mockClientPolicy.Object, mockSettings.Object);
    }

    [Fact]
    public void Get_Returns_Given_StatusCode()
    {
        var response = _service.Get(200);

        Equal(200, response.Result);
    }

    [Fact]
    public async Task TestGetWithRetryPolicy_Success()
    {
        var result = await _service.GetWithRetryPolicy(500, 200);

        Equal(200, result);
    }

    [Fact]
    public async Task TestGetWithRetryPolicy_Fail()
    {
        var result = await _service.GetWithRetryPolicy(500);

        Equal(500, result);
    }
}