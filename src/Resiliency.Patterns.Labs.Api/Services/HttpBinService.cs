using Resiliency.Patterns.Labs.Api.Configuration;
using Resiliency.Patterns.Labs.Api.Services.Interfaces;

using Serilog;

namespace Resiliency.Patterns.Labs.Api.Services;

public class HttpBinService : IHttpBinService
{
    private readonly HttpClient _httpClient;

    private readonly ClientPolicy _clientPolicy;

    private const string BASE_URI = "http://httpbin.org/status";
    
    public HttpBinService(HttpClient httpClient, ClientPolicy clientPolicy)
    {
        _httpClient = httpClient;
        _clientPolicy = clientPolicy;
    }
    
    public async Task<int> Get(int statusCode)
    {
        var response = await _httpClient.GetAsync($"{BASE_URI}/{statusCode}");
        
        Log.Information($"{response.IsSuccessStatusCode}");
        
        return (int)response.StatusCode;
    }

    public async Task<int> GetWithRetryPolicy(params int[] statusCode)
    {
        var response = await _clientPolicy.ExponentialHttpRetry.ExecuteAsync(()
            => _httpClient.GetAsync($"{BASE_URI}/{string.Join(",", statusCode)}"));

        Log.Information(response.IsSuccessStatusCode
            ? "--> [Retry] HttpBinService returned a Success"
            : "--> [Retry] HttpBinService returned a FAILURE");
        
        return (int)response.StatusCode;
    }

    public async Task<int> GetWithCircuitBreakerPolicy(int statusCode)
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                _ = await _clientPolicy.CircuitBreakerPolicy.ExecuteAsync(async ()
                    =>
                {
                    var result = await _httpClient.GetAsync($"{BASE_URI}/{string.Join(",", statusCode)}");
                    result.EnsureSuccessStatusCode();
                    return result;
                });
            }
            catch (Exception ex)
            {
                Log.Error($"[Circuit Break] Exception caught: {ex.Message}");
            }
            
            // to simulate CircuitBreak.OnReset
            await Task.Delay(1000);
        }

        return statusCode;
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