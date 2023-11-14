using System.Net;

using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Fallback;
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
    
    public AsyncBulkheadPolicy BulkheadPolicy { get; }
    
    public AsyncFallbackPolicy<HttpResponseMessage> FallbackPolicy { get; }
 
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
        
        BulkheadPolicy = Policy.BulkheadAsync(1, 3, onBulkheadRejectedAsync: (context) =>
        {
            Log.Error("[Bulkhead] Execution and queue slots full. Requests will be rejected.");
            return Task.CompletedTask;
        });

        FallbackPolicy =  Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .FallbackAsync(fallbackAction: (response, context, cancellationToken) =>  {
                Log.Error($"[Fallback] Action is executing. ");
                HttpResponseMessage httpResponseMessage = new(HttpStatusCode.UnprocessableEntity)
                {
                    Content = new StringContent($"[Fallback] The fallback executed, the original error was {response.Result.ReasonPhrase}")
                };
                return Task.FromResult(httpResponseMessage);
            }, onFallbackAsync: (result, context) =>   {
                Log.Error("[Fallback] About to call the fallback action.");
                return Task.CompletedTask;
            });
    }
}