using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

/// <summary>
/// Request DTO for rejecting an ownership request.
/// </summary>
public class ReqRejectOwnershipRequestDto
{
    [MaxLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
    public string? RejectionReason { get; set; }
}