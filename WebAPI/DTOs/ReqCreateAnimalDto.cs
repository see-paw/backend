namespace WebAPI.DTOs;

using Domain;
using Domain.Enums;

public class ReqCreateAnimalDto
{
    public string Name { get; set; } = null!;//this property is set during model binding.
    public Species Species { get; set; }
    public string BreedId { get; set; } = null!;
    public SizeType Size { get; set; }
    public SexType Sex { get; set; }
    public string Colour { get; set; } = null!;
    public DateOnly BirthDate { get; set; }
    public bool Sterilized { get; set; }
    public decimal Cost { get; set; }
    public string? Features { get; set; } 
    public string? Description { get; set; } 
    public List<ReqImageDTO>? Images { get; set; }
}
