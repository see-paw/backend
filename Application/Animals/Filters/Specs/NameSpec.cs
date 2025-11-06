using System.Linq.Expressions;
using Application.Interfaces;
using Domain;

namespace Application.Animals.Filters.Specs;

/// <summary>
/// Specification that filters animals by name.
/// </summary>
public class NameSpec : ISpecification<Animal>
{
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Checks if the animal's name contains the specified text.
    /// Case-insensitive partial match.
    /// </summary>
    public bool IsSatisfied(Animal t)
    {
        if (string.IsNullOrWhiteSpace(Name))
            return true;

        if (string.IsNullOrWhiteSpace(t.Name))
            return false;

        return t.Name.Contains(Name.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts the name specification to a database-queryable expression.
    /// Returns null if no name filter is specified to avoid unnecessary query conditions.
    /// </summary>
    public Expression<Func<Animal, bool>>? ToExpression()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return null;

        var name = Name.Trim().ToLower();
        return animal => animal.Name.ToLower().Contains(name);
    }
}