using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Ownership;

/// <summary>
/// Request DTO for rejecting an ownership request.
/// </summary>
public class ReqRejectOwnershipRequestDto
{
    public string? RejectionReason { get; set; }
}