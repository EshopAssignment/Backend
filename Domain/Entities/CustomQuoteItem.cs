
namespace Domain.Entities;

public class CustomQuoteItem
{
    public int Id { get; set; }

    public int CustomQuoteId { get; set; }
    public CustomQuote CustomQuote { get; set; } = null!;

    public string Description { get; set; } = null!;
    public int Quantity { get; set; }

    public int VatRatePercent { get; set; }
    public decimal UnitPriceExVat { get; set; }
    public decimal UnitVatAmount { get; set; }
    public decimal UnitPriceIncVat { get; set; }

    public decimal LineTotalExVat { get; set; }
    public decimal LineTotalVat { get; set; }
    public decimal LineTotalIncVat { get; set; }
}
