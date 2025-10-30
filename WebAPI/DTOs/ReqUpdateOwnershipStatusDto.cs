using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

/// <summary>
/// Request DTO for updating ownership request status from Pending to Analysing.
/// </summary>
public class ReqUpdateOwnershipStatusDto
{
    public string? RequestInfo { get; set; }
}