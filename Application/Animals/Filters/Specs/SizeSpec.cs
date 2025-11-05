using System.Linq.Expressions;
using Application.Interfaces;
using Domain;
using Domain.Enums;

namespace Application.Animals.Filters.Specs;

/// <summary>
/// Specification that filters animals by size.
/// </summary>
public class SizeSpec : ISpecification<Animal>
{
    public SizeType Size { get; init; }

    /// <summary>
    /// Checks if the animal's size matches the specified size.
    /// </summary>
    public bool IsSatisfied(Animal t)
    {
        return Size == t.Size;
    }

    /// <summary>
    /// Converts the size specification to a database-queryable expression.
    /// </summary>
    public Expression<Func<Animal, bool>>? ToExpression()
    {
        return animal => animal.Size == Size; 
    }
}