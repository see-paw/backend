using System.Linq.Expressions;
using Application.Interfaces;
using Domain;
using Domain.Enums;

namespace Application.Animals.Filters.Specs;

/// <summary>
/// Specification that filters animals by species.
/// </summary>
public class SpeciesSpec : ISpecification<Animal>
{
    public Species Species { get; init; }

    /// <summary>
    /// Checks if the animal's species matches the specified species.
    /// </summary>
    public bool IsSatisfied(Animal t)
    {
        return t.Species.Equals(Species); 
    }

    /// <summary>
    /// Converts the species specification to a database-queryable expression.
    /// </summary>
    public Expression<Func<Animal, bool>>? ToExpression()
    {
        return animal => animal.Species == Species; 
    }
}