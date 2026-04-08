using Application.DTOs.Blob;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Services;

public class BlobUploadService
{
    private readonly BlobContainerClient _containerClient;

    public BlobUploadService(IConfiguration config)
    {
        var connectionString = config["BlobStorage:ConnectionString"];
        var containerName = config["BlobStorage:ContainerName"];

        var serviceClient = new BlobServiceClient(connectionString);
        _containerClient = serviceClient.GetBlobContainerClient(containerName);
    }

    public (Uri UploadUri, string publicUrl, string blobName) CreateUploadSas(string contentType)
    {
        var blobName = $"products/original/{Guid.NewGuid()}.webp";

        var blobClient = _containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return (sasUri, blobClient.Uri.ToString(), blobName);
    }

    public async Task<ProcessedImageDto> ProcessImageAsync(string blobName, CancellationToken ct)
    {
        var originalBlob = _containerClient.GetBlobClient(blobName);

        if (!await originalBlob.ExistsAsync(ct))
            throw new FileNotFoundException("Uploaded blob was not found.", blobName);

        await using var originalStream = new MemoryStream();
        await originalBlob.DownloadToAsync(originalStream, ct);
        originalStream.Position = 0;

        using var image = await Image.LoadAsync(originalStream, ct);

        var fileName = Path.GetFileNameWithoutExtension(blobName);

        var largeUrl = await SaveVariantAsync(image, $"products/large/{fileName}.webp", maxWidth: 1400, ct);
        var cardUrl = await SaveVariantAsync(image, $"products/card/{fileName}.webp", maxWidth: 700, ct);
        var stackUrl = await SaveVariantAsync(image, $"products/stack/{fileName}.webp", maxWidth: 480, ct);
        var thumbUrl = await SaveVariantAsync(image, $"products/thumb/{fileName}.webp", maxWidth: 160, ct);

        return new ProcessedImageDto(
            originalBlob.Uri.ToString(),
            largeUrl,
            cardUrl,
            stackUrl,
            thumbUrl
        );
    }

    private async Task<string> SaveVariantAsync(Image source, string blobName, int maxWidth, CancellationToken ct)
    {
        using var clone = source.Clone(ctx =>
        {
            ctx.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxWidth, 0)
            });
        });

        await using var ms = new MemoryStream();
        await clone.SaveAsWebpAsync(ms, new WebpEncoder
        {
            Quality = 78
        }, ct);

        ms.Position = 0;

        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(ms, overwrite: true, cancellationToken: ct);

        return blobClient.Uri.ToString();
    }
}
