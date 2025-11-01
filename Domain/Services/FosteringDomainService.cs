using Domain.Enums;

namespace Domain.Services;

public class FosteringDomainService
{
    public decimal GetAnimalCurrentSupport(Animal animal)
    {
        return animal.Fosterings
            .Where(f => f.Status == FosteringStatus.Active)
            .Sum(f => f.Amount);
    }
    
    public bool IsAlreadyFosteredByUser(Animal animal, string userId)
    {
        return animal.Fosterings.Any(f => f.UserId == userId && f.Status == FosteringStatus.Active);
    }
}