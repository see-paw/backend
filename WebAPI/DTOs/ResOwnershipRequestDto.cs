using Domain.Enums;

namespace WebAPI.DTOs;

/// <summary>
/// Response DTO for ownership request data.
/// </summary>
public class ResOwnershipRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string AnimalId { get; set; } = string.Empty;
    public string AnimalName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public OwnershipStatus Status { get; set; }
    public string? RequestInfo { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}