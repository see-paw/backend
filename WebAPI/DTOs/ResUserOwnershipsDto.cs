using Domain.Enums;

namespace WebAPI.DTOs;

public class ResUserOwnershipsDto
{
    public string Id { get; set; } = string.Empty;
    public string AnimalId { get; set; } = string.Empty;
    public string AnimalName { get; set; } = string.Empty;
    public AnimalState AnimalState { get; set; }
    public ResImageDto Image { get; set; } = null!;
    public decimal Amount { get; set; }
    public OwnershipStatus? OwnershipStatus { get; set; }
    public string? RequestInfo { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}  