using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Ownership;

/// <summary>
/// Request DTO for creating a new ownership request.
/// </summary>
public class ReqCreateOwnershipRequestDto
{
    [Required(ErrorMessage = "Animal ID is required")]
    public string AnimalId { get; set; } = string.Empty;
}