using Domain;
using Domain.Enums;

namespace Persistence.Seeds;

/// <summary>
/// Seeds ownership requests into the database.
/// </summary>
internal static class OwnershipRequestSeeder
{
    /// <summary>
    /// Seeds all ownership requests into the database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (!dbContext.OwnershipRequests.Any())
        {
            var ownershipRequests = new List<OwnershipRequest>
            {
                new()
                {
                    Id = SeedConstants.OwnershipRequest1Id,
                    AnimalId = SeedConstants.OwnershipAnimal1Id,
                    UserId = SeedConstants.OwnershipUser1Id,
                    Amount = 100,
                    Status = OwnershipStatus.Pending,
                    RequestInfo = "Tenho experiência com cães desta raça",
                    RequestedAt = DateTime.UtcNow.AddDays(-5)
                },
                new()
                {
                    Id = SeedConstants.OwnershipRequest2Id,
                    AnimalId = SeedConstants.OwnershipAnimal2Id,
                    UserId = SeedConstants.OwnershipUser1Id,
                    Amount = 90,
                    Status = OwnershipStatus.Pending,
                    RequestInfo = "Procuro uma companheira calma",
                    RequestedAt = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    Id = SeedConstants.OwnershipRequest3Id,
                    AnimalId = SeedConstants.OwnershipAnimal3Id,
                    UserId = SeedConstants.OwnershipUser1Id,
                    Amount = 120,
                    Status = OwnershipStatus.Rejected,
                    RequestInfo = "Tenho quintal grande",
                    RequestedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new()
                {
                    Id = SeedConstants.OwnershipRequest4Id,
                    AnimalId = SeedConstants.OwnershipAnimal7Id,
                    UserId = SeedConstants.OwnershipUser1Id,
                    Amount = 80,
                    Status = OwnershipStatus.Rejected,
                    RequestInfo = "Quero um cão jovem",
                    RequestedAt = DateTime.UtcNow.AddDays(-50),
                    UpdatedAt = DateTime.UtcNow.AddDays(-40)
                },
                new()
                {
                    Id = SeedConstants.OwnershipRequest5Id,
                    AnimalId = SeedConstants.OwnershipAnimal8Id,
                    UserId = SeedConstants.OwnershipUser1Id,
                    Amount = 95,
                    Status = OwnershipStatus.Approved,
                    RequestInfo = "Perfeita para o meu estilo de vida",
                    RequestedAt = DateTime.UtcNow.AddDays(-60),
                    ApprovedAt = DateTime.UtcNow.AddDays(-55),
                    UpdatedAt = DateTime.UtcNow.AddDays(-55)
                }
            };

            await dbContext.OwnershipRequests.AddRangeAsync(ownershipRequests);
            await dbContext.SaveChangesAsync();
        }
    }
}