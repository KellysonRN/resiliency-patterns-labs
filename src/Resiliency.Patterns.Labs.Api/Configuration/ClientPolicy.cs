using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

using Serilog;

namespace Resiliency.Patterns.Labs.Api.Configuration;

public class ClientPolicy
{
    public AsyncRetryPolicy<HttpResponseMessage> ImmediateHttpRetry { get;}
    
    public AsyncRetryPolicy<HttpResponseMessage> LinearHttpRetry {get;}
    
    public AsyncRetryPolicy<HttpResponseMessage> ExponentialHttpRetry {get;}
    
    public AsyncCircuitBreakerPolicy CircuitBreakerPolicy { get; }
    
    public AsyncTimeoutPolicy TimeoutPolicyPessimistic { get;  }
    
    public AsyncBulkheadPolicy BulkheadPolicyAsync { get; }
 
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
                Log.Error($"Attempt: {retryAttempt}");
                return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
            });
        
        CircuitBreakerPolicy = Policy
            .HandleInner<HttpRequestException>()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(3),
                onBreak: (_, _) =>
                {
                    Log.Error($"[Circuit Break] Circuit open, too many failures, requests blocked.");
                },
                onReset: () =>
                {
                    Log.Error("[Circuit Break] Circuit closed, request allowed.");
                },
                onHalfOpen: () =>
                {
                    Log.Error("[Circuit Break] Circuit test, one request will be allowed.");
                }
            );
        
        TimeoutPolicyPessimistic = Policy.TimeoutAsync(1, TimeoutStrategy.Pessimistic, onTimeoutAsync: (_, _, _) =>
            {
                Log.Error("[Timeout] Polly's timeout pessimistic policy terminated request because it was taking too long.");
                return Task.CompletedTask;
            });
        
        BulkheadPolicyAsync = Policy.BulkheadAsync(1, 3, onBulkheadRejectedAsync: (context) =>
        {
            Log.Error("[Bulkhead] Execution and queue slots full. Requests will be rejected.");
            return Task.CompletedTask;
        });
    }
}