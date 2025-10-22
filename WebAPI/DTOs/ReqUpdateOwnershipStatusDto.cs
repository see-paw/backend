using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

/// <summary>
/// Request DTO for updating ownership request status from Pending to Analysing.
/// </summary>
public class ReqUpdateOwnershipStatusDto
{
    [MaxLength(500, ErrorMessage = "Request info cannot exceed 500 characters")]
    public string? RequestInfo { get; set; }
}