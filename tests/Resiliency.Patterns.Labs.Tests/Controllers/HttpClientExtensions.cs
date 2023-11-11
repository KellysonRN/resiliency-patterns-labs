using Newtonsoft.Json;

namespace Resiliency.Patterns.Labs.Tests.Controllers;

internal static class HttpClientExtensions
{
    public static async Task<T?> DoGetAsync<T>(this HttpClient client, string uri)
        where T : class
    {
        var response = await client.GetAsync(uri).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<T>(responseContent);

        return result;
    }
}

public class RestResult<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = "error";
    public string ErrorCode { get; set; } = "-1";

    public static RestResult<T> Ok(T data)
    {
        return new RestResult<T>
        {
            Data = data,
            Success = true
        };
    }

    public static RestResult<T> Error(string code, string message)
    {
        return new RestResult<T>
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message
        };
    }
}