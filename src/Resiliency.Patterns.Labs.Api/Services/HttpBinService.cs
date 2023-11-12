using Resiliency.Patterns.Labs.Api.Configuration;
using Resiliency.Patterns.Labs.Api.Services.Interfaces;

namespace Resiliency.Patterns.Labs.Api.Services;

public class HttpBinService : IHttpBinService
{
    private readonly HttpClient _httpClient;

    private readonly ILogger<HttpBinService> _logger;

    private readonly ClientPolicy _clientPolicy;

    private const string BASE_URI = "http://httpbin.org/status";
    
    public HttpBinService(HttpClient httpClient, ILogger<HttpBinService> logger, ClientPolicy clientPolicy)
    {
        _httpClient = httpClient;
        _logger = logger;
        _clientPolicy = clientPolicy;
    }
    
    public async Task<int> Get(int statusCode)
    {
        var response = await _httpClient.GetAsync($"{BASE_URI}/{statusCode}");
        
        _logger.LogInformation($"{response.IsSuccessStatusCode}");
        
        return (int)response.StatusCode;
    }

    public async Task<int> GetWithRetryPolicy(params int[] statusCode)
    {
        var response = await _clientPolicy.ExponentialHttpRetry.ExecuteAsync(()
            => _httpClient.GetAsync($"{BASE_URI}/{string.Join(",", statusCode)}"));

        _logger.LogInformation(response.IsSuccessStatusCode
            ? "--> HttpBinService returned a Success"
            : "--> HttpBinService returned a FAILURE");
        
        return (int)response.StatusCode;
    }

    public async Task<int> GetWithCircuitBreakerPolicy(int statusCode)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetWithTimeoutPolicy(int statusCode)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetWithBulkheadIsolation(int statusCode)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetWithFallbackPolicy(int statusCode)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetWithWrappingThePolicies(int statusCode)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetWithRetry(int statusCode)
    {
        throw new NotImplementedException();
    }
}