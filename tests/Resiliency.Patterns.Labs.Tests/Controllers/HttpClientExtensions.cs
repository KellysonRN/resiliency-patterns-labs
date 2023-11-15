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