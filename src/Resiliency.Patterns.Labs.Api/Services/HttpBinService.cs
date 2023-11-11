using Resiliency.Patterns.Labs.Api.Services.Interfaces;

namespace Resiliency.Patterns.Labs.Api.Services;

public class HttpBinService : IHttpBinService
{
    private readonly HttpClient _httpClient;

    private readonly ILogger<HttpBinService> _logger;
    
    public HttpBinService(HttpClient httpClient, ILogger<HttpBinService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<int> Get(int statusCode)
    {
        var response = await _httpClient.GetAsync($"http://httpbin.org/status/{statusCode}");
        
        _logger.LogInformation($"{response.IsSuccessStatusCode}");
        
        return (int)response.StatusCode;
    }
}