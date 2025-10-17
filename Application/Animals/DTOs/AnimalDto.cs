using Domain.Enums;


namespace Application.Animals.DTOs;

public class AnimalDto
{
    public string Name { get; set; } = "";
    public AnimalState AnimalState { get; set; }

    public string? Description { get; set; }
    public Species Species { get; set; }
    public SizeType Size { get; set; }
    public SexType Sex { get; set; }
    public string Colour { get; set; } = "";
    public DateOnly BirthDate { get; set; }
    public bool Sterilized { get; set; }
    public Breed Breed { get; set; }
    public decimal Cost { get; set; }
    public string Features { get; set; } = "";
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    public string MainImageUrl { get; set; } = "";
}