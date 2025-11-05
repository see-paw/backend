namespace Application.Animals.Filters;

/// <summary>
/// Model representing filter criteria for querying animals.
/// This model is framework-agnostic and used in the Application layer.
/// </summary>
public class AnimalFilterModel
{
    public string? Species { get; set; }
    public int? Age { get; set; }
    public string? Size { get; set; }
    public string? Sex { get; set; }
    public string? Name { get; set; }
    public string? ShelterName { get; set; }
    public string? Breed { get; set; }
}