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
    public IActionResult RequestUpload([FromBody] RequestUploadDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FileName))
            return BadRequest("Invalid file Name");

        if(!dto.ContentType.StartsWith("image/"))
            return BadRequest("Only image uploads are allowed");


        var (uploadUri, publicUrl) = _blobService.CreateUploadSas(dto.ContentType);
        return Ok(new BlobUploadRequestResponse(uploadUri.ToString(), publicUrl));
    }
}
