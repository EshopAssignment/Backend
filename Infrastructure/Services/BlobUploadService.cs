using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;

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

    public (Uri UploadUri, string publicUrl) CreateUploadSas(string contentType)
    {
        var blobName = $"products/{Guid.NewGuid()}.webp";

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
        return (sasUri, blobClient.Uri.ToString());
    }
}
