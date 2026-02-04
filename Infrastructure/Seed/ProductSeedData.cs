
using Domain.Enums;

namespace Infrastructure.Seed;

public static class ProductSeedData
{
    public static readonly IReadOnlyList<ProductTemplate> Templates =
    [
        new("EU-pall", ProductType.EuroPallet),
        new("Industripall", ProductType.IndustrialPallet),
        new("Halvpall", ProductType.HalfPallet),
        new("Engångspall", ProductType.Other),
        new("Värmebehandlad pall", ProductType.SpecialPallet)
    ];

    public record ProductTemplate(
        string BaseName,
        ProductType Type
    );
}
