using Application.DTOs.Blob;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BlobUploadController(BlobUploadService blobService) : ControllerBase
{
    private readonly BlobUploadService _blobService = blobService;

    [HttpPost("request")]
    [ProducesResponseType(typeof(BlobUploadRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public IActionResult RequestUpload([FromBody] RequestUploadDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FileName))
            return BadRequest("Invalid file name");

        if (string.IsNullOrWhiteSpace(dto.ContentType) || !dto.ContentType.StartsWith("image/"))
            return BadRequest("Only image uploads are allowed");

        var (uploadUri, publicUrl, blobName) = _blobService.CreateUploadSas(dto.ContentType);
        return Ok(new BlobUploadRequestResponse(uploadUri.ToString(), publicUrl, blobName));
    }

    [HttpPost("finalize")]
    [ProducesResponseType(typeof(ProcessedImageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FinalizeUpload([FromBody] FinalizeBlobUploadDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.BlobName))
            return BadRequest("BlobName is required");

        var result = await _blobService.ProcessImageAsync(dto.BlobName, ct);
        return Ok(result);
    }
}
