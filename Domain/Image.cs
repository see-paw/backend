<<<<<<< HEAD
﻿using System.ComponentModel.DataAnnotations;
=======
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

>>>>>>> feature/create-and-list-animals
namespace Domain;

public class Image
{
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [Required]
    public bool IsPrincipal { get; set; }

<<<<<<< HEAD
    // Foreign Key
    public string? AnimalId { get; set; }

    public Animal? Animal { get; set; }
    public string? ShelterId { get; set; }
=======
    // Foreign Key for animal
    public string? AnimalId { get; set; } 

    [JsonIgnore]
    public Animal? Animal { get; set; }

    // Foreign Key for Shelter 
    public string? ShelterId { get; set; } 
    [JsonIgnore]
>>>>>>> feature/create-and-list-animals
    public Shelter? Shelter { get; set; }

    [Required]
    [MaxLength(500)]
<<<<<<< HEAD
    public string Url { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }
=======
    public string Url { get; set; } = null!;

    [MaxLength(255)]
    public string? Description { get; set; } 
>>>>>>> feature/create-and-list-animals

    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}