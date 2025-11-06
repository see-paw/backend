using System.Linq.Expressions;

namespace Domain.Services;

public class AnimalDomainService
{
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
    /// Expression to get Animal's Age
    /// </summary>
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