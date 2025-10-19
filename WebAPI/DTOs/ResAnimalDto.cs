using Domain;
using Domain.Enums;

namespace WebAPI.DTOs;
public class ResAnimalDto
{
    public required string Id { get; set; }        
    public required string Name { get; set; }
    public required Species Species { get; set; }
    public required SizeType Size { get; set; }
    public required SexType Sex { get; set; }
    public required AnimalState AnimalState { get; set; }
    public required string Colour { get; set; }
    public required DateOnly BirthDate { get; set; }
    public int Age { get; set; }                         
    public string Description { get; set; } = string.Empty;
    public required bool Sterilized { get; set; }
    public string Features { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string BreedName { get; set; } = string.Empty;

    public ICollection<ResImageDto> Images { get; set; } = [];
}

