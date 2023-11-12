namespace Resiliency.Patterns.Labs.Api.Services.Interfaces;

public interface IHttpBinService
{
    Task<int> Get(int statusCode);

    Task<int> GetWithRetryPolicy(params int[] statusCode);
    
    Task<int> GetWithCircuitBreakerPolicy(int statusCode);
    
    Task<int> GetWithTimeoutPolicy(int statusCode);
    
    Task<int> GetWithBulkheadIsolation(int statusCode);
    
    Task<int> GetWithFallbackPolicy(int statusCode);
    
    Task<int> GetWithWrappingThePolicies(int statusCode);
}