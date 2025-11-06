using System.Linq.Expressions;

namespace Application.Interfaces;

/// <summary>
/// Specification pattern interface for filtering entities.
/// </summary>
/// <typeparam name="T">The entity type to filter</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Checks if the entity satisfies the specification.
    /// Used for in-memory filtering and unit testing.
    /// </summary>
    /// <param name="t">The entity to check</param>
    /// <returns>True if the entity satisfies the specification</returns>
    bool IsSatisfied(T t);

    /// <summary>
    /// Converts the specification to a LINQ expression for database queries.
    /// Returns null if the specification should not be applied as a filter.
    /// </summary>
    /// <returns>Expression to use in database queries, or null if not applicable</returns>
    Expression<Func<T, bool>>? ToExpression();
}