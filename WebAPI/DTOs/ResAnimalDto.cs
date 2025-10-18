using Domain.Enums;

namespace WebAPI.DTOs;

public class ResAnimalDto
{
    public required string AnimalId { get; set; }
    public required string Name { get; set; }
    public required Species Species { get; set; }
    public required SizeType Size { get; set; }
    public required SexType Sex { get; set; }
    public required ResBreedDto Breed { get; set; }
    public required AnimalState AnimalState { get; set; }
    public required string Colour { get; set; }
    public required DateOnly BirthDate { get; set; }
    public int Age { get; set; }
    public string? Description { get; set; }
    public required bool Sterilized { get; set; }
    public string? Features { get; set; }
    public decimal Cost { get; set; }

    public String shelterId { get; set; }
    //public List<ResImageDto>? Images { get; set; }
}
