using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.Services;
using MediatR;

namespace Application.Services;

public class FosteringService(FosteringDomainService fosteringDomainService) : IFosteringService
{
    const string InvalidAnimalState = "Invalid animal state";
    const string AnimalHasAnOwnerNotAvailableForFostering = "Animal has an owner, not available for fostering";
    const string AnimalIsTotallyFostered = "Animal is totally fostered";
    const string AnimalIsInactive = "Animal is inactive";
    
    public Result<Unit> isInValidStateForFostering(Animal animal)
    {
        if (animal.AnimalState is not (AnimalState.Inactive
            or AnimalState.TotallyFostered
            or AnimalState.HasOwner)) return Result<Unit>.Success(Unit.Value, 200);
        
        var (message, code) = animal.AnimalState switch
        {
            AnimalState.Inactive => (AnimalIsInactive, 409),
            AnimalState.TotallyFostered => (AnimalIsTotallyFostered, 409),
            AnimalState.HasOwner => (AnimalHasAnOwnerNotAvailableForFostering, 409),
            _ => (InvalidAnimalState, 400)
        };

        return Result<Unit>.Failure(message, code);
    }
    
    public void UpdateFosteringState(Animal animal)
    {
        if (animal.Fosterings.Count == 0)
        {
            animal.AnimalState = AnimalState.Available;
            return;
        }

        var total = fosteringDomainService.GetAnimalCurrentSupport(animal);

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