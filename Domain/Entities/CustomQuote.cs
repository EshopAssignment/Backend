
using Domain.Enums;

namespace Domain.Entities;

public class CustomQuote
{
    public int Id { get; set; }

    public int CustomRequestId { get; set; }
    public CustomRequest CustomRequest { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SentAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; set; }

    public string Title { get; set; } = null!;
    public string Currency { get; set; } = "SEK";
    public string? CustomerMessage { get; set; }
    public string? InternalNote { get; set; }

    public CustomQuoteStatus Status { get; private set; } = CustomQuoteStatus.Draft;

    public decimal SubtotalExVat { get; private set; }
    public decimal VatTotal { get; private set; }
    public decimal TotalIncVat { get; private set; }

    public ICollection<CustomQuoteItem> Items { get; set; } = [];

    public void ReplaceItems(IEnumerable<CustomQuoteItem> items)
    {
        Items.Clear();
        foreach (var item in items)
            Items.Add(item);

        RecalculateTotals();
        Touch();
    }

    public void MarkSent()
    {
        Status = CustomQuoteStatus.Sent;
        SentAtUtc = DateTime.UtcNow;
        Touch();
    }

    public void MarkAccepted()
    {
        Status = CustomQuoteStatus.Accepted;
        Touch();
    }

    public void MarkRejected()
    {
        Status = CustomQuoteStatus.Rejected;
        Touch();
    }

    public void MarkExpired()
    {
        Status = CustomQuoteStatus.Expired;
        Touch();
    }

    private void RecalculateTotals()
    {
        SubtotalExVat = Items.Sum(x => x.LineTotalExVat);
        VatTotal = Items.Sum(x => x.LineTotalVat);
        TotalIncVat = Items.Sum(x => x.LineTotalIncVat);
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
