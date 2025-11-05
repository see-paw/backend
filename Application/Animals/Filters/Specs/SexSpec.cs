using System.Linq.Expressions;
using Application.Interfaces;
using Domain;
using Domain.Enums;

namespace Application.Animals.Filters.Specs;

/// <summary>
/// Specification that filters animals by sex.
/// </summary>
public class SexSpec : ISpecification<Animal>
{
    public SexType Sex { get; init; }

    /// <summary>
    /// Checks if the animal's sex matches the specified sex.
    /// </summary>
    public bool IsSatisfied(Animal t)
    {
        return Sex == t.Sex;
    }

    /// <summary>
    /// Converts the sex specification to a database-queryable expression.
    /// </summary>
    public Expression<Func<Animal, bool>>? ToExpression()
    {
        return animal => animal.Sex == Sex;
    }
}