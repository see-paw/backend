using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WebAPI.DTOs.Animals;

public class AnimalFilterDto
{
    public string? Species { get; set; }
    public int? Age { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? Sex { get; set; }
    public string? Name { get; set; }
    public string? ShelterName { get; set; }
    public string? Breed { get; set; }
}