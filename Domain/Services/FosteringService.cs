namespace Domain.Services;

public class FosteringService
{
    public decimal GetAnimalCurrentSupport(Animal animal)
    {
        return animal.Fosterings.Sum(f => f.Amount);
    }
    
    public bool IsAlreadyFosteredByUser(Animal animal, string userId)
    {
        return animal.Fosterings.Any(f => f.UserId == userId);
    }
    
}