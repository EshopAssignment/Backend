using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [MaxLength(2048)]
    public string OriginalUrl { get; set; } = null!;
    [MaxLength(2048)]
    public string LargeUrl { get; set; } = null!;
    [MaxLength(2048)]
    public string CardUrl { get; set; } = null!;
    [MaxLength(2048)]
    public string StackUrl { get; set; } = null!;
    [MaxLength(2048)]
    public string ThumbUrl { get; set; } = null!;



    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }

    [MaxLength(100)]
    public string? AltText { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
