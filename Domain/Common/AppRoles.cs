namespace Domain.Common;

/// <summary>
/// Defines the different user roles available in the SeePaw application.
/// These roles are used for access control, authorization,
/// and managing permissions across the system.
/// </summary>
public static class AppRoles
{
    /// <summary>
    /// Represents the platform administrator role,
    /// which has full access to all entities and operations within the application.
    /// </summary>
    public const string PlatformAdmin = "PlatformAdmin";
    
    /// <summary>
    /// Represents the administrator role for an Animal Shelter (CAA),
    /// responsible for managing animals, requests, and internal shelter operations.
    /// </summary>
    public const string AdminCAA = "AdminCAA";
    
    /// <summary>
    /// Represents the standard user role,
    /// with permissions limited to browsing, fostering, adoption,
    /// and ownership request actions.
    /// </summary>
    public const string User = "User";
    
    /// <summary>
    /// Gets a collection containing all roles available in the application.
    /// </summary>
    public static IEnumerable<string> All => new[] { PlatformAdmin, AdminCAA, User };
}