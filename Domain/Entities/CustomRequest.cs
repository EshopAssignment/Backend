
using Domain.Enums;

namespace Domain.Entities;

public class CustomRequest
{
    public int id {  get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string Messsage { get; set; } = null!;
    
    public CustomRequestStatus Status { get; private set; } = CustomRequestStatus.New;

    public string? AttatchemntName { get; set; }
    public string? AttatchemtBlobPath { get; set; }

    public string? InternalNote { get; set; }
    public ICollection<CustodmQuote> Quotes { get; set; } = [];

    public void MarkReviewed()
    {
        Status = CustomRequestStatus.Reviewed;
        Touch();
    }

    public void MarkQuoted()
    {
        Status = CustomRequestStatus.Quoted;
        Touch();
    }

    public void MarkClosed()
    {
        Status = CustomRequestStatus.Closed;
        Touch();
    }

    public void MarkRejected()
    {
        Status = CustomRequestStatus.Rejected;
        Touch();
    }

    public void SetInternalNote(string? note)
    {
        InternalNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
