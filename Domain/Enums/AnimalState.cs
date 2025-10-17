namespace Domain.Enums
{
    /// <summary>
    /// Represents the current adoption or fostering state of an animal.
    /// </summary>
    /// <remarks>
    /// This enumeration defines the lifecycle states that describe the availability
    /// and adoption/fostering status of an animal within the SeePaw system.
    /// </remarks>
    public enum AnimalState
    {
        
        Available,// The animal is available for ownership or fostering.
        PartiallyFostered,// The animal is currently being partially fostered (not fully sponsored).
        TotallyFostered,// The animal is fully fostered (completely sponsored).
        HasOwner, // The animal has been adopted and has an official owner.
        Inactive  // The animal record is inactive (e.g., unavailable or deceased).
    }
}
