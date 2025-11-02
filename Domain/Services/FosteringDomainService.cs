using Domain.Enums;

namespace Domain.Services;

/// <summary>
/// Provides domain-level utility operations related to fostering,
/// including calculations and checks that support business logic in higher layers.
/// </summary>
public class FosteringDomainService
{
    /// <summary>
    /// Calculates the total active monthly support amount for a given <see cref="Animal"/>.
    /// </summary>
    /// <param name="animal">The animal entity whose active fostering contributions should be summed.</param>
    /// <returns>
    /// A <see cref="decimal"/> value representing the total monthly amount contributed
    /// by all active fosterings associated with the specified animal.
    /// </returns>
    public decimal GetAnimalCurrentSupport(Animal animal)
    {
        return animal.Fosterings
            .Where(f => f.Status == FosteringStatus.Active)
            .Sum(f => f.Amount);
    }
    
    /// <summary>
    /// Checks whether the specified user already has an active fostering
    /// for the given <see cref="Animal"/>.
    /// </summary>
    /// <param name="animal">The animal entity to check against.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// <c>true</c> if the user currently fosters the animal; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAlreadyFosteredByUser(Animal animal, string userId)
    {
        return animal.Fosterings.Any(f => f.UserId == userId && f.Status == FosteringStatus.Active);
    }
}