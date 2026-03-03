namespace Application.DTOs.Blob;

public sealed record RequestUploadDto(
    string FileName, string ContentType
);
