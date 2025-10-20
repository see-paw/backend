namespace Domain.Enums;

/// <summary>
/// Represents the types of activities that can occur within the system.
/// </summary>
/// <remarks>
/// Defines whether the activity refers to the fostering or ownership of an animal.
/// </remarks>
public enum ActivityType
{
    /// <summary>
    /// Activity related to fostering an animal, where the user provides recurring support.
    /// </summary>
    Fostering,

    /// <summary>
    /// Activity related to owning an animal, establishing a full and permanent responsibility.
    /// </summary>
    Ownership
}
