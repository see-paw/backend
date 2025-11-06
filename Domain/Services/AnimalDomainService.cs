using System.Linq.Expressions;

namespace Domain.Services;

/// <summary>
/// Domain service responsible for operations related to animals,
/// such as age calculation and age-based expressions for filtering.
/// </summary>
public class AnimalDomainService
{
    /// <summary>
    /// Calculates the current age of the specified animal based on its birth date.
    /// </summary>
    /// <param name="animal">The animal whose age is to be calculated.</param>
    /// <returns>The age of the animal in years.</returns>
    public int GetAge(Animal animal)
    {
        var todayDate = DateOnly.FromDateTime(DateTime.Now);

        if (animal.BirthDate.Month > todayDate.Month || 
            (animal.BirthDate.Month == todayDate.Month && animal.BirthDate.Day > todayDate.Day))
        {
            return todayDate.Year - animal.BirthDate.Year - 1;
        }
        
        return todayDate.Year - animal.BirthDate.Year;
    }
    
    /// <summary>
    /// Builds a LINQ expression that checks if an animal’s age equals a given value.
    /// Used for database queries to avoid in-memory evaluation.
    /// </summary>
    /// <param name="age">The target age to match.</param>
    /// <returns>
    /// A LINQ expression returning true when the animal’s calculated age equals the specified value.
    /// </returns>
    public Expression<Func<Animal, bool>> GetAgeEqualsExpression(int age)
    {
        var today = DateTime.UtcNow;
        var year = today.Year;
        var month = today.Month;
        var day = today.Day;
        
        return animal =>
            (animal.BirthDate.Month < month ||
             (animal.BirthDate.Month == month && animal.BirthDate.Day <= day)
                ? (year - animal.BirthDate.Year)
                : (year - animal.BirthDate.Year - 1)) == age;
    }
}