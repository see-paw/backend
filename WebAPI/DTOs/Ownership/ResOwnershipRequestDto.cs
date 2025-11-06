using Domain.Enums;

namespace WebAPI.DTOs.Ownership;

/// <summary>
/// Response DTO for ownership request data.
/// </summary>
public class ResOwnershipRequestDto
{
    /// <summary>
    /// Unique identifier of the ownership request.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifier of the associated animal.
    /// </summary>
    public string AnimalId { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the associated animal.
    /// </summary>
    public string AnimalName { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifier of the user who submitted the request.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the user who submitted the request.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Amount paid or pledged for the ownership process.
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Current status of the ownership request.
    /// </summary>
    public OwnershipStatus Status { get; set; }
    
    /// <summary>
    /// Optional information or notes attached to the request.
    /// </summary>
    public string? RequestInfo { get; set; }
    
    /// <summary>
    /// Date and time when the request was created.
    /// </summary>
    public DateTime RequestedAt { get; set; }
    
    /// <summary>
    /// Date and time when the request was approved, if applicable.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    /// <summary>
    /// Date and time when the request was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}