namespace WebAPI.DTOs;

using Domain;
using Domain.Enums;

public class ReqCreateAnimalDto
{
    public required string Name { get; set; }
    public required Species Species { get; set; }
    public required string BreedId { get; set; }
    public required SizeType Size { get; set; }
    public required SexType Sex { get; set; }
    public required string Colour { get; set; }
    public required DateOnly BirthDate { get; set; }
    public required bool Sterilized { get; set; }
    public required decimal Cost { get; set; }
    public string? Features { get; set; }
    public string? Description { get; set; }

    //public List<ReqImageDto>? Images { get; set; } é preciso criar um DTO para imagens
}
