using Application.Core;
using Domain;
using MediatR;

namespace Application.Interfaces;

/// <summary>
/// Defines operations related to the fostering business logic at the application level.
/// </summary>
public interface IFosteringService
{
    /// <summary>
    /// Updates the <see cref="Domain.Animal"/> fostering state based on the current
    /// total amount of active fosterings associated with it.
    /// </summary>
    /// <param name="animal">The animal entity whose fostering state will be evaluated and updated.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the total fostering contributions exceed the animal’s cost.
    /// </exception>
    public void UpdateFosteringState(Animal animal);
    
    /// <summary>
    /// Validates whether a given <see cref="Domain.Animal"/> is in a valid state
    /// to receive a new fostering.
    /// </summary>
    /// <param name="animal">The animal entity to validate.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> object indicating success if the animal can be fostered,
    /// or failure with an appropriate error message and HTTP-like code if not.
    /// </returns>
    public Result<Unit> isInValidStateForFostering(Animal animal);
}