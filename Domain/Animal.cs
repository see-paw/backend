using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain;

public class Animal
{
    [Key] public string AnimalId { get; set; } = Guid.NewGuid().ToString();

    public required string Name { get; set; }

    public required DateOnly BirthDate { get; set; }
}
