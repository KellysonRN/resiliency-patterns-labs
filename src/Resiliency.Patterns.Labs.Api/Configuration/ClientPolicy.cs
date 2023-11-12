using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

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
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(3));

        ExponentialHttpRetry = Policy.HandleResult<HttpResponseMessage>(
                res => !res.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt =>
            {
                Console.WriteLine($"Attempt: {retryAttempt}");
                return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
            });
        
        CircuitBreakerPolicy = Policy
            .HandleInner<HttpRequestException>()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(3),
                onBreak: (ex, timespan) =>
                {
                    Console.WriteLine($"[Circuit Break] Circuit open, too many failures, requests blocked.");
                },
                onReset: () =>
                {
                    Console.WriteLine("[Circuit Break] Circuit closed, request allowed.");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("[Circuit Break] Circuit test, one request will be allowed.");
                }
            );
    }
}