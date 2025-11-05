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
}