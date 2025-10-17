using API.DTOs;
using AutoMapper;
using Domain;

namespace API.Core;

/// <summary>
/// AutoMapper configuration profile that defines the object-to-object mappings.
/// AutoMapper uses these mappings to automatically convert between different types,
/// avoiding repetitive manual property assignment code.
/// This class is automatically discovered and registered when calling AddAutoMapper().
/// </summary>
public class MappingProfiles : Profile
{
    /// <summary>
    /// Constructor where all application mappings are defined.
    /// </summary>
    public MappingProfiles()
    {
        // Mapping: Animal → Animal
        // Used to clone/copy Animal objects.
        // Useful when is necessary to create a duplicate of an entity without affecting the original.
        CreateMap<Animal, Animal>();

        // Mapping: CreateAnimalDTO → Animal
        // Converts the DTO received from the client into the domain entity
        CreateMap<CreateAnimalDTO, Animal>();
    }
}


