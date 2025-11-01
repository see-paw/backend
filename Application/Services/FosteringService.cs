using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.Services;
using MediatR;

namespace Application.Services;

/// <summary>
/// Provides application-level logic for managing fostering operations,
/// including validation of animal states and updating their fostering status.
/// </summary>
public class FosteringService(FosteringDomainService fosteringDomainService) : IFosteringService
{
    const string InvalidAnimalState = "Invalid animal state";
    const string AnimalHasAnOwnerNotAvailableForFostering = "Animal has an owner, not available for fostering";
    const string AnimalIsTotallyFostered = "Animal is totally fostered";
    const string AnimalIsInactive = "Animal is inactive";
    
    /// <summary>
    /// Determines whether a given <see cref="Domain.Animal"/> is in a valid state
    /// to be fostered by a user.
    /// </summary>
    /// <param name="animal">The animal entity to validate.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> object representing:
    /// <list type="bullet">
    /// <item><description><c>Success (200)</c> if the animal can be fostered.</description></item>
    /// <item><description><c>Failure (409)</c> if the animal is inactive, has an owner, or is already totally fostered.</description></item>
    /// <item><description><c>Failure (400)</c> for any other invalid state.</description></item>
    /// </list>
    /// </returns>
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
    
    /// <summary>
    /// Updates the fostering state of a given <see cref="Domain.Animal"/> 
    /// based on the total active fostering contributions.
    /// </summary>
    /// <param name="animal">The animal entity whose fostering state is to be updated.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the total monthly fostering value exceeds the animal’s defined cost.
    /// </exception>
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