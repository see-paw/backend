using Domain;

namespace Persistence.Seeds;

/// <summary>
/// Seeds favorites into the database.
/// </summary>
internal static class FavoriteSeeder
{
    /// <summary>
    /// Seeds all favorites into the database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (!dbContext.Favorites.Any())
        {
            var favorites = new List<Favorite>
            {
                new()
                {
                    Id = SeedConstants.Favorite1Id,
                    UserId = SeedConstants.User7Id,
                    AnimalId = SeedConstants.Animal12Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    Id = SeedConstants.Favorite2Id,
                    UserId = SeedConstants.User7Id,
                    AnimalId = SeedConstants.Animal13Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    Id = SeedConstants.Favorite3Id,
                    UserId = SeedConstants.User7Id,
                    AnimalId = SeedConstants.Animal14Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    Id = SeedConstants.Favorite4Id,
                    UserId = SeedConstants.User7Id,
                    AnimalId = SeedConstants.Animal1Id,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                }
            };

            await dbContext.Favorites.AddRangeAsync(favorites);
            await dbContext.SaveChangesAsync();
        }
    }
}