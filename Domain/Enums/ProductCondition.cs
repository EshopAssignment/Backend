
using System.ComponentModel;

namespace Domain.Enums;

public enum ProductCondition
{
    [Description("Ny")] New = 1,
    [Description("Begangnad")] Used = 2,
    [Description("Upprustad")] Refurbished = 3
}
