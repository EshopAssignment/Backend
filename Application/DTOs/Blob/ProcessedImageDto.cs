namespace Application.DTOs.Blob;

public sealed record ProcessedImageDto(
    string OriginalUrl,
    string LargeUrl,
    string CardUrl,
    string StackUrl,
    string ThumbUrl
    );
