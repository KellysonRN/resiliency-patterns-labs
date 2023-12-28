using Microsoft.Extensions.Options;

using Polly;

using Resiliency.Patterns.Labs.Api.Configuration;
using Resiliency.Patterns.Labs.Api.Services.Interfaces;

using Serilog;

namespace Resiliency.Patterns.Labs.Api.Services;

public class HttpBinService : IHttpBinService
{
    private readonly HttpClient _httpClient;

    private readonly ClientPolicy _clientPolicy;

    private readonly string? _uri;

    public static CancellationTokenSource CancellactionToken = new();

    public static Context Context = new("KeyForSomething");

    public HttpBinService(HttpClient httpClient, ClientPolicy clientPolicy, IOptions<HttpBinSettings> httpBinSettings)
    {
        _httpClient = httpClient;
        _clientPolicy = clientPolicy;

        _uri = httpBinSettings.Value.Uri;
    }

    public async Task<int> Get(int statusCode)
    {
        var response = await _httpClient.GetAsync($"{_uri}/{statusCode}");

        Log.Information($"{response.IsSuccessStatusCode}");

        return (int)response.StatusCode;
    }

    public async Task<int> GetWithRetryPolicy(params int[] statusCode)
    {
        var response = await _clientPolicy.ExponentialHttpRetry.ExecuteAsync(
            () => _httpClient.GetAsync($"{_uri}/{string.Join(",", statusCode)}")
        );

        Log.Information(
            response.IsSuccessStatusCode
                ? "--> [Retry] HttpBinService returned a Success"
                : "--> [Retry] HttpBinService returned a FAILURE"
        );

        return (int)response.StatusCode;
    }

    public async Task<int> GetWithCircuitBreakerPolicy(int statusCode)
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                _ = await _clientPolicy.CircuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var result = await _httpClient.GetAsync(
                        $"{_uri}/{string.Join(",", statusCode)}"
                    );
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
        await _clientPolicy.TimeoutPolicy.ExecuteAsync(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            var result = _httpClient.GetAsync($"{_uri}/{statusCode}");
            return result;
        });

        return statusCode;
    }

    public async Task<int> GetWithBulkheadIsolation(int statusCode)
    {
        List<Task> tasks = new();

        for (int i = 1; i <= 10; i++)
        {
            CustomProcessor(i);
            Thread.Sleep(500);
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Log.Error($"[Bulkhead] Some bulkhead tasks failed with exception: {ex.Message}\n");
        }

        for (int loop = 0; loop < 10; loop++)
        {
            Log.Error($"[Bulkhead] Task {loop}: {tasks[loop].Status}");
        }

        return statusCode;

        void CustomProcessor(int num)
        {
            Log.Error(
                $"[Bulkhead] Execution slots: {_clientPolicy.BulkheadPolicy.BulkheadAvailableCount}, Queue Slots: {_clientPolicy.BulkheadPolicy.QueueAvailableCount}"
            );

            var response = _clientPolicy.BulkheadPolicy.ExecuteAsync(async () =>
            {
                Log.Error($"[Bulkhead] Executing caller to HttpBin service: ({num})");

                await Task.Delay(TimeSpan.FromSeconds(3));
                var result = _httpClient.GetAsync($"{_uri}/{statusCode}");
                return result;
            });

            tasks.Add(response);
        }
    }

    public async Task<int> GetWithFallbackPolicy(int statusCode)
    {
        var response = await _clientPolicy.FallbackPolicy.ExecuteAsync(async () =>
        {
            var result = await _httpClient.GetAsync($"{_uri}/{string.Join(",", statusCode)}");
            return result;
        });

        Log.Error($"[Fallback] {await response.Content.ReadAsStringAsync()}");

        return (int)response.StatusCode;
    }

    public async Task<int> GetWithCachePolicy(int statusCode)
    {
        var response = await _clientPolicy.CachePolicy.ExecuteAsync(
            action: async (_, ct) =>
            {
                var result = await _httpClient.GetAsync(
                    $"{_uri}/{string.Join(",", statusCode)}", ct);
                return result;
            }, context: Context,
       cancellationToken: CancellactionToken.Token,
       continueOnCapturedContext: false);

        Log.Error(
            $"[CachePolicy] result={response.ReasonPhrase}."
        );

        return statusCode;
    }

    public async Task<int> GetWithWrappingThePolicies(int statusCode)
    {
        var response = await _clientPolicy.PolicyWrap.ExecuteAsync(async () =>
        {
            var result = await _httpClient.GetAsync($"{_uri}/{string.Join(",", statusCode)}");
            await Task.Delay(3000);
            return result;
        });

        Log.Error($"[Wrap] {await response.Content.ReadAsStringAsync()}");

        return statusCode;
    }

    public void CancelarToken()
    {
        Log.Error(
            $"[CachePolicy] DELETE CACHE"
        );

        CancellactionToken.Cancel();

        CancellactionToken.Dispose();

        Context.Remove("KeyForSomething");

        CancellactionToken = new CancellationTokenSource();

        Log.Error(
            $"[CachePolicy] INIT CACHE"
        );

    }
}