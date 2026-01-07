using System.Text.Json;
using Application.DTOs.Shipping;
using Application.Interfaces;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class PostNordClient(HttpClient http, IOptions<PostNordOptions> opt) : IPostNordClient
{
    private readonly PostNordOptions _opt = opt.Value;

    public async Task<IReadOnlyList<ServicePointDto>> FindServicePointsAsync(string postalCode, string? city, CancellationToken ct)
    {
        var q = new List<string>
    {
        $"apikey={Uri.EscapeDataString(_opt.ApiKey)}",
        "countryCode=SE",
        $"postalCode={Uri.EscapeDataString(postalCode)}",
        "numberOfServicePoints=10",
        "returnType=json"
    };

        if (!string.IsNullOrWhiteSpace(city))
            q.Add($"city={Uri.EscapeDataString(city)}");

        var url = "v5/servicepoints/bypostalcode?" + string.Join("&", q);

        using var res = await http.GetAsync(url, ct);

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"PostNord {(int)res.StatusCode} {res.ReasonPhrase}. Body: {body}");
        }

        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var root = doc.RootElement;

        if (!TryGet(root, out var arr, "servicePointInformationResponse", "servicePoints") &&
            !TryGet(root, out arr, "servicePoints") &&
            !TryGet(root, out arr, "servicePointInformationResponse", "servicePoint"))
        {
            return Array.Empty<ServicePointDto>();
        }

        var outList = new List<ServicePointDto>();

        foreach (var sp in arr.EnumerateArray())
        {
            var id = GetStr(sp, "servicePointId") ?? GetStr(sp, "id");
            var name = GetStr(sp, "name") ?? GetStr(sp, "servicePointName") ?? "";

            var street = "";
            var pc = "";
            var c = "";

            if (sp.TryGetProperty("visitingAddress", out var va) && va.ValueKind == JsonValueKind.Object)
            {
                street =
                    GetStr(va, "streetName") ??
                    GetStr(va, "street") ??
                    "";

                var streetNo = GetStr(va, "streetNumber");
                if (!string.IsNullOrWhiteSpace(streetNo) && !street.Contains(streetNo))
                    street = (street + " " + streetNo).Trim();

                pc = GetStr(va, "postalCode") ?? "";
                c = GetStr(va, "city") ?? "";
            }
            else if (sp.TryGetProperty("address", out var a) && a.ValueKind == JsonValueKind.Object)
            {
                street = GetStr(a, "street") ?? "";
                pc = GetStr(a, "postalCode") ?? "";
                c = GetStr(a, "city") ?? "";
            }

            if (string.IsNullOrWhiteSpace(id))
                continue;

            outList.Add(new ServicePointDto(
                id.Trim(),
                name.Trim(),
                street.Trim(),
                pc.Trim(),
                c.Trim()
            ));
        }

        return outList;
    }


    private static bool TryGet(JsonElement el, out JsonElement found, params string[] path)
    {
        found = el;
        foreach (var p in path)
        {
            if (found.ValueKind != JsonValueKind.Object || !found.TryGetProperty(p, out found))
                return false;
        }
        return found.ValueKind == JsonValueKind.Array;
    }

    private static string? GetStr(JsonElement el, string prop)
        => el.ValueKind == JsonValueKind.Object && el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;
}
