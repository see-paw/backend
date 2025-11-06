namespace Application.Animals.Filters;

/// <summary>
/// Model representing filter criteria for querying animals.
/// This model is framework-agnostic and used in the Application layer.
/// </summary>
public class AnimalFilterModel
{
    /// <summary>
    /// Animal species (e.g., dog, cat).
    /// </summary>
    public string? Species { get; set; }
    
    /// <summary>
    /// Animal age in years.
    /// </summary>
    public int? Age { get; set; }
    
    /// <summary>
    /// Animal size (e.g., small, medium, large).
    /// </summary>
    public string? Size { get; set; }
    
    /// <summary>
    /// Animal sex (e.g., male, female).
    /// </summary>
    public string? Sex { get; set; }
    
    /// <summary>
    /// Animal name or part of the name.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Name of the shelter (CAA).
    /// </summary>
    public string? ShelterName { get; set; }
    
    /// <summary>
    /// Animal breed.
    /// </summary>
    public string? Breed { get; set; }
}