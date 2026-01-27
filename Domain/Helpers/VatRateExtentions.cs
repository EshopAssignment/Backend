

using Domain.Enums;

namespace Domain.Helpers;

public static class VatRateExtentions
{
    public static decimal ToMultiplier(this VatRate rate)
        => (decimal)(int)rate / 100m;

    public static decimal ToPercent(this VatRate rate)
        => (decimal)(int)rate;
}
