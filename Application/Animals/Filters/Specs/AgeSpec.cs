using System.Linq.Expressions;
using Application.Interfaces;
using Domain;
using Domain.Services;

namespace Application.Animals.Filters.Specs;

/// <summary>
/// Specification that filters animals by age.
/// </summary>
public class AgeSpec(AnimalDomainService animalDomainService) : ISpecification<Animal>
{
    /// <summary>
    /// Age value used to filter animals.
    /// </summary>
    public int Age { get; init; }

    /// <summary>
    /// Checks if the animal's age matches the specified age.
    /// Uses AnimalDomainService for accurate age calculation.
    /// </summary>
    public bool IsSatisfied(Animal t)
    {
        return animalDomainService.GetAge(t) == Age;
    }

    /// <summary>
    /// Converts the age specification to a database-queryable expression.
    /// Calculates age directly in the expression to avoid method call translation issues.
    /// </summary>
    public Expression<Func<Animal, bool>>? ToExpression()
    {
        return animalDomainService.GetAgeEqualsExpression(Age);
    }
}