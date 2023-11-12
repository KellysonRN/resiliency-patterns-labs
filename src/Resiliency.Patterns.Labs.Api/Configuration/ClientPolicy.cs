using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

using Serilog;

namespace Resiliency.Patterns.Labs.Api.Configuration;

public class ClientPolicy
{
    public AsyncRetryPolicy<HttpResponseMessage> ImmediateHttpRetry { get;}
    
    public AsyncRetryPolicy<HttpResponseMessage> LinearHttpRetry {get;}
    
    public AsyncRetryPolicy<HttpResponseMessage> ExponentialHttpRetry {get;}
    
    public AsyncCircuitBreakerPolicy CircuitBreakerPolicy { get; }
 
    public ClientPolicy()
    {
        ImmediateHttpRetry = Policy.HandleResult<HttpResponseMessage>(
                res => !res.IsSuccessStatusCode)
            .RetryAsync(10);

        LinearHttpRetry = Policy.HandleResult<HttpResponseMessage>(
                res => !res.IsSuccessStatusCode)
            .WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(3));

        ExponentialHttpRetry = Policy.HandleResult<HttpResponseMessage>(
                res => !res.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt =>
            {
                Log.Information($"Attempt: {retryAttempt}");
                return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
            });
        
        CircuitBreakerPolicy = Policy
            .HandleInner<HttpRequestException>()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(3),
                onBreak: (_, _) =>
                {
                    Log.Information($"[Circuit Break] Circuit open, too many failures, requests blocked.");
                },
                onReset: () =>
                {
                    Log.Information("[Circuit Break] Circuit closed, request allowed.");
                },
                onHalfOpen: () =>
                {
                    Log.Information("[Circuit Break] Circuit test, one request will be allowed.");
                }
            );
    }
}