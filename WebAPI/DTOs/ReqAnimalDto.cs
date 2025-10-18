namespace WebAPI.DTOs;

using Domain.Enums;

public class ReqAnimalDto
{
    public required string Name { get; set; }
    public AnimalState? AnimalState { get; set; }
    public string? Description { get; set; }
    public required Species Species { get; set; }
    public required Breed Breed { get; set; }
    public required SizeType Size { get; set; }
    public required SexType Sex { get; set; }
    public required string Colour { get; set; }
    public required DateOnly BirthDate { get; set; }
    public required bool Sterilized { get; set; }
    public required decimal Cost { get; set; }
    public string Features { get; set; } = string.Empty;
    public required string MainImageUrl { get; set; }
}
