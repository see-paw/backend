using Domain.Enums;

namespace Domain.Services;

public class FosteringService
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
    
    public void UpdateFosteringState(Animal animal)
    {
        if (animal.Fosterings.Count == 0)
        {
            animal.AnimalState = AnimalState.Available;
            return;
        }

        var total = GetAnimalCurrentSupport(animal);

        if (total < animal.Cost)
            animal.AnimalState = AnimalState.PartiallyFostered;
        else if (total == animal.Cost)
            animal.AnimalState = AnimalState.TotallyFostered;
        else
        {
            throw new InvalidOperationException("Monthly value surpasses animal costs");
        }
    }
    
}