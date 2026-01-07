using System.Net.NetworkInformation;
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
            "numberOfServicePoints=10"
        };
        if (!string.IsNullOrWhiteSpace(city))
            q.Add($"city={Uri.EscapeDataString(city)}");

        var url = "v5/servicepoints?" + string.Join("&", q);

        using var res = await http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();

        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var root = doc.RootElement;

        JsonElement listEl;

        if (TryGet(root, out listEl, "servicePointInformationResponse", "servicePoints") ||
            TryGet(root, out listEl, "servicePoints") ||
            TryGet(root, out listEl, "servicePointInformationResponse", "servicePoint"))
        {
            var outList = new List<ServicePointDto>();
            foreach (var sp in listEl.EnumerateArray())
            {
                var id = GetStr(sp, "servicePointId") ?? GetStr(sp, "id") ?? "";
                var name = GetStr(sp, "name") ?? GetStr(sp, "servicePointName") ?? "";
                var visit = sp.TryGetProperty("visitingAddress", out var va) ? va : default;

                var street = visit.ValueKind != JsonValueKind.Undefined ? (GetStr(va, "streetName") ?? GetStr(va, "street")) : "";
                var pc = visit.ValueKind != JsonValueKind.Undefined ? (GetStr(va, "postalCode") ?? "") : "";
                var c = visit.ValueKind != JsonValueKind.Undefined ? (GetStr(va, "city") ?? "") : "";

                if (!string.IsNullOrWhiteSpace(id))
                    outList.Add(new ServicePointDto(id, name, street, pc, c));
            }
            return outList;
        }

        return [];

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
