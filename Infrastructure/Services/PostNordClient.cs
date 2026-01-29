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
        var pcClean = new string((postalCode ?? "").Where(char.IsDigit).ToArray());
        if (pcClean.Length != 5)
            return [];

        var q = new List<string>
    {
        $"apikey={Uri.EscapeDataString(_opt.ApiKey)}",
        "countryCode=SE",
        $"postalCode={Uri.EscapeDataString(pcClean)}",
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


            if ((int)res.StatusCode == 400 && LooksLikeNoServicePointFound(body))
                return [];

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
            if (string.IsNullOrWhiteSpace(id))
                continue;

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

    private static bool LooksLikeNoServicePointFound(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return false;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("servicePointInformationResponse", out var sp) &&
                sp.TryGetProperty("compositeFault", out var cf) &&
                cf.TryGetProperty("faults", out var faults) &&
                faults.ValueKind == JsonValueKind.Array)
            {
                foreach (var f in faults.EnumerateArray())
                {
                    if (f.TryGetProperty("faultCode", out var fc) &&
                        fc.ValueKind == JsonValueKind.String &&
                        string.Equals(fc.GetString(), "noServicePointFound", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
        }
        catch
        {
            Console.WriteLine("KENTA VAFAN GÖR DU!??!??!?");
        }

        return body.Contains("noServicePointFound", StringComparison.OrdinalIgnoreCase);
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
