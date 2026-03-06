using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [MaxLength(2048)]
    public string Url { get; set; } = null!;

    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }

    [MaxLength(100)]
    public string? AltText { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
