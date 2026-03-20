using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Order;


public sealed record CreateCustomRequestFormDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = "";

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; init; } = "";

    [Phone]
    [MaxLength(50)]
    public string Phone { get; init; } = "";

    [Required]
    [MaxLength(4000)]
    public string Message { get; init; } = "";

    public IFormFile? File { get; init; }
}



