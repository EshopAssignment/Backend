
using System.Text.Json;

namespace IntegrationTests.Helpers;

public static class HttpResponseExtensions
{
    private static readonly JsonSerializerOptions JsonOpt = new(JsonSerializerDefaults.Web);

    public static async Task<T> ReadJsonAsync<T>(this HttpResponseMessage res)
    {
        var txt = await res.Content.ReadAsStringAsync();
        if(!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"HTTP {(int)res.StatusCode}:\n{txt}");

        var obj = JsonSerializer.Deserialize<T>(txt, JsonOpt);
        return obj ?? throw new InvalidOperationException("Could Not deserialize response");
    }
}
