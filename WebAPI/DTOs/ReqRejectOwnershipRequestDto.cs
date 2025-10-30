using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

/// <summary>
/// Request DTO for rejecting an ownership request.
/// </summary>
public class ReqRejectOwnershipRequestDto
{
    public string? RejectionReason { get; set; }
}