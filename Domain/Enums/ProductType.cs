

using System.ComponentModel;

namespace Domain.Enums;

public enum ProductType
{
    [Description("EURO-pall")] EuroPallet = 1,
    [Description("Halv-pall")] HalfPallet = 2,
    [Description("Industri-pall")] IndustrialPallet = 3,
    [Description("Specialmåttad")] CustomPallet = 4,
    [Description("Speciall-pall")] SpecialPallet = 5,
    [Description("Övrigt")] Other = 6
}
