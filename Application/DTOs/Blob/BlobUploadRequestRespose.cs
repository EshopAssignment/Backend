namespace Application.DTOs.Blob;

public sealed record BlobUploadRequestResponse(string UploadUrl, string PublicUrl, string BlobName);
