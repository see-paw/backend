using API.DTOs;
using AutoMapper;
using Domain;

namespace API.Core;

/// <summary>
/// Defines AutoMapper configuration profiles for object-to-object mappings used in the application.
/// </summary>
/// <remarks>
/// The <see cref="MappingProfiles"/> class specifies how domain entities are transformed into Data Transfer Objects (DTOs)
/// and vice versa.  
/// 
/// It centralizes mapping configurations to ensure consistent and maintainable conversions across layers.
/// </remarks>
public class MappingProfiles : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingProfiles"/> class
    /// and registers the mapping between <see cref="Domain.Animal"/> and <see cref="API.DTOs.AnimalDto"/>.
    /// </summary>
    /// <remarks>
    /// This configuration allows AutoMapper to automatically convert <see cref="Domain.Animal"/> entities
    /// into <see cref="API.DTOs.AnimalDto"/> objects during query and response handling.
    /// </remarks>
    public MappingProfiles()
    {
        CreateMap<Animal, AnimalDto>();
    }
}
