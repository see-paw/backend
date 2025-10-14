using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.DTOs;

public class BaseAnimalDTO
{
    public string Name { get; set; } = "";

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
    public string MainImageUrl { get; set; } = "";
}

