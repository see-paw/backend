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
        // Useful when you need to create a duplicate of an entity without affecting the original.
        CreateMap<Animal, Animal>();

        // Mapping: CreateAnimalDTO → Animal
        // Converts the DTO received from the client (data for creation) into the domain entity.
        // Used in CreateAnimal.Handler to transform the DTO into an Animal object before saving it to the database.
        // Example: { Name: "Bobby", Age: 3 } → new Animal { Name = "Bobby", Age = 3 }
        CreateMap<CreateAnimalDTO, Animal>();
    }
}


