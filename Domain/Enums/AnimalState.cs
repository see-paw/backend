namespace Domain.Enums;

/// <summary>
/// Represents the possible states of an animal within the system.
/// </summary>
/// <remarks>
/// Used to indicate the animal’s current availability for fostering or ownership,
/// and to manage visibility and interaction within the platform.
/// </remarks>
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
    /// The animal is fully fostered and not available for additional sponsorships.
    /// </summary>
    TotallyFostered,

    /// <summary>
    /// The animal has an owner and is no longer available for fostering.
    /// </summary>
    HasOwner,

    /// <summary>
    /// The animal is inactive or temporarily unavailable for public viewing.
    /// </summary>
    Inactive
}
