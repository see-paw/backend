using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

/// <summary>
/// Request DTO for creating a new ownership request.
/// </summary>
public class ReqCreateOwnershipRequestDto
{
    [Required(ErrorMessage = "Animal ID is required")]
    public string AnimalId { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Request info cannot exceed 500 characters")]
    public string? RequestInfo { get; set; }
}