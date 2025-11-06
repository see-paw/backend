using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Ownership;

/// <summary>
/// Request DTO for creating a new ownership request.
/// </summary>
public class ReqCreateOwnershipRequestDto
{
    /// <summary>
    /// Unique identifier of the animal for which the ownership request is made.
    /// </summary>
    public string AnimalId { get; set; } = string.Empty;
}