using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Ownership;

/// <summary>
/// Request DTO for rejecting an ownership request.
/// </summary>
public class ReqRejectOwnershipRequestDto
{
    /// <summary>
    /// Optional reason explaining why the ownership request was rejected.
    /// </summary>
    public string? RejectionReason { get; set; }
}
