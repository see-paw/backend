namespace Application.Common;

public static class AppRoles
{
    public const string PlatformAdmin = "PlatformAdmin";
    public const string AdminCAA = "AdminCAA";
    public const string User = "User";
    
    public static IEnumerable<string> All => new[] { PlatformAdmin, AdminCAA, User };
}