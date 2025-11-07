using System.Linq.Expressions;
using Application.Interfaces;
using Domain;

namespace Application.Animals.Filters.Specs;

/// <summary>
/// Specification that filters animals by shelter name.
/// </summary>
public class ShelterNameSpec : ISpecification<Animal>
{
    /// <summary>
    /// Shelter name used for filtering (case-insensitive).
    /// </summary>
    public string ShelterName { get; init; } = string.Empty;

    /// <summary>
    /// Checks if the animal's shelter name contains the specified text.
    /// Case-insensitive partial match.
    /// </summary>
    public bool IsSatisfied(Animal t)
    {
        if (string.IsNullOrWhiteSpace(ShelterName)) 
            return true; 

        if (t.Shelter == null || string.IsNullOrWhiteSpace(t.Shelter.Name))
            return false;

        return t.Shelter.Name.Contains(ShelterName.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts the shelter name specification to a database-queryable expression.
    /// Returns null if no shelter filter is specified to avoid unnecessary query conditions.
    /// </summary>
    public Expression<Func<Animal, bool>>? ToExpression()
    {
        if (string.IsNullOrWhiteSpace(ShelterName))
            return null;
        
        var shelterName = ShelterName.Trim().ToLower();
        return animal => animal.Shelter.Name.ToLower().Contains(shelterName);
    }
}