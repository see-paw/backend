using System.Linq.Expressions;
using Application.Interfaces;
using Domain;

namespace Application.Animals.Filters.Specs;

/// <summary>
/// Specification that filters animals by breed name.
/// </summary>
public class BreedSpec : ISpecification<Animal>
{
    public string BreedName { get; init; } = string.Empty;

    /// <summary>
    /// Checks if the animal's breed contains the specified breed name.
    /// Case-insensitive partial match.
    /// </summary>
    public bool IsSatisfied(Animal t)
    {
        if (string.IsNullOrWhiteSpace(BreedName))
            return true;

        if (string.IsNullOrWhiteSpace(t.Breed.Name))
            return false;

        return t.Breed.Name.Contains(BreedName.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts the breed specification to a database-queryable expression.
    /// Returns null if no breed filter is specified to avoid unnecessary query conditions.
    /// </summary>
    public Expression<Func<Animal, bool>>? ToExpression()
    {
        if (string.IsNullOrWhiteSpace(BreedName))
            return null;

        var breedName = BreedName.Trim().ToLower();
        return a => a.Breed.Name.ToLower().Contains(breedName);
    }
}