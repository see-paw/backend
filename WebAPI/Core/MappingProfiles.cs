using AutoMapper;
using Domain;
using WebAPI.DTOs;

namespace WebAPI.Core;

/// <summary>
/// Defines AutoMapper configuration profiles for mapping between domain entities and Data Transfer Objects (DTOs).
/// </summary>
/// <remarks>
/// This profile handles mappings between <see cref="Animal"/>, <see cref="ReqAnimalDto"/>,
/// <see cref="ResAnimalDto"/>, and <see cref="ResImageDto"/>.  
/// It includes computed and relational mappings such as age calculation and breed name resolution.
/// </remarks>
public class MappingProfiles : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingProfiles"/> class and defines all mapping configurations.
    /// </summary>
    /// <remarks>
    /// Configures the mappings between domain entities and DTOs used in the application:
    /// <list type="bullet">
    /// <item><description><see cref="ReqAnimalDto"/> → <see cref="Animal"/></description></item>
    /// <item><description><see cref="Animal"/> → <see cref="ResAnimalDto"/> (includes computed fields such as <c>Age</c> and <c>BreedName</c>)</description></item>
    /// <item><description><see cref="Image"/> → <see cref="ResImageDto"/></description></item>
    /// </list>
    /// These mappings are used by AutoMapper to transform objects between the application and API layers.
    /// </remarks>
    public MappingProfiles()
    {
        CreateMap<ReqCreateAnimalDto, Animal>()
            // Maps the 'Images' collection from the request DTO to the 'Images' navigation property in the Animal domain entity
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));


        CreateMap<Breed, ResBreedDto>();

        CreateMap<Animal, ResAnimalDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Age, 
                opt => opt.MapFrom(src => DateTime.Today.Year - src.BirthDate.Year))
            .ForMember(dest => dest.Breed,
                opt => opt.MapFrom(src => src.Breed))
            .ForMember(dest => dest.Images,
                opt => opt.MapFrom(src => src.Images));

        CreateMap<ReqImageDto, Image>();
      
        CreateMap<Image, ResImageDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));// Maps the 'Id' property from Image to the 'ImageId' property in ResImageDto.
        


    }
}