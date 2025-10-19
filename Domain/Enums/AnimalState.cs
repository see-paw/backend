namespace Domain.Enums;

/// <summary>
/// Represents the current state of an animal within the system.
/// </summary>
public enum AnimalState
{
    /// <summary>
    /// The animal is available for ownership or fostering.
    /// </summary>
    Available,

    /// <summary>
    /// The animal is currently being partially fostered.
    /// </summary>
    PartiallyFostered,

    /// <summary>
    /// The animal is fully fostered and not available for new sponsorships.
    /// </summary>
    TotallyFostered,

    /// <summary>
    /// The animal has an owner and is not available for fostering.
    /// </summary>
    HasOwner,

    /// <summary>
    /// The animal is inactive or temporarily unavailable for public viewing.
    /// </summary>
    Inactive
}