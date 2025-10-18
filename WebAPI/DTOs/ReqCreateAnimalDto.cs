namespace WebAPI.DTOs;

using Domain;
using Domain.Enums;

public class ReqCreateAnimalDto
{
    public string Name { get; set; }
    public Species Species { get; set; }
    public string BreedId { get; set; }
    public SizeType Size { get; set; }
    public SexType Sex { get; set; }
    public string Colour { get; set; }
    public DateOnly BirthDate { get; set; }
    public bool Sterilized { get; set; }
    public decimal Cost { get; set; }
    public string? Features { get; set; }
    public string? Description { get; set; }

    //public List<ReqImageDto>? Images { get; set; } é preciso criar um DTO para imagens
}
