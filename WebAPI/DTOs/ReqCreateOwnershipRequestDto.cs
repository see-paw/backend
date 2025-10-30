using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

/// <summary>
/// Request DTO for creating a new ownership request.
/// </summary>
public class ReqCreateOwnershipRequestDto
{
    public string AnimalId { get; set; } = string.Empty;
}