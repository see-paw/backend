using Domain.Enums;

namespace WebAPI.DTOs;

public class ResAnimalDto
{
    public  string AnimalId { get; set; } = null!;
    public  string Name { get; set; } = null!;
    public  Species Species { get; set; }
    public  SizeType Size { get; set; }
    public  SexType Sex { get; set; }
    public  ResBreedDto Breed { get; set; }
    public  AnimalState AnimalState { get; set; }
    public  string Colour { get; set; } = null!;
    public  DateOnly BirthDate { get; set; }
    public int Age { get; set; }
    public string? Description { get; set; }
    public  bool Sterilized { get; set; }
    public string? Features { get; set; }
    public  decimal Cost { get; set; }
    public  string shelterId { get; set; } = null!;
    public List<ResImageDto>? Images { get; set; }


}
