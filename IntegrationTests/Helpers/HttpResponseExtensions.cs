
using System.Net.Http.Headers;
using System.Text;
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


    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string url, T body)
        => client.PostAsync(url, new StringContent(JsonSerializer.Serialize(body, JsonOpt), Encoding.UTF8, "application/json"));

    public static Task<HttpResponseMessage> PatchJsonAsync<T>(this HttpClient client, string url, T body)
    {
        var req = new HttpRequestMessage(HttpMethod.Patch, url);
        req.Content = new StringContent(JsonSerializer.Serialize(body, JsonOpt), Encoding.UTF8, "application/json");
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client.SendAsync(req);
    }


}
