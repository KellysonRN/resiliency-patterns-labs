using Polly;
using Polly.Retry;

namespace Resiliency.Patterns.Labs.Api.Configuration;

public class ClientPolicy
{
    public AsyncRetryPolicy<HttpResponseMessage> ImmediateHttpRetry { get;}
    
    public AsyncRetryPolicy<HttpResponseMessage> LinearHttpRetry {get;}
    
    public AsyncRetryPolicy<HttpResponseMessage> ExponentialHttpRetry {get;}
 
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
    }
}