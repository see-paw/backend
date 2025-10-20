using AutoMapper;
using Domain;
using WebAPI.DTOs;

namespace WebAPI.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ReqCreateAnimalDto, Animal>()
            // Maps the 'Images' collection from the request DTO to the 'Images' navigation property in the Animal domain entity
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));


        CreateMap<Breed, ResBreedDto>();

        CreateMap<Animal, ResAnimalDto>()
            .ForMember(dest => dest.AnimalId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Age, 
                opt => opt.MapFrom(src => DateTime.Today.Year - src.BirthDate.Year))
            .ForMember(dest => dest.Breed,
                opt => opt.MapFrom(src => src.Breed))
            .ForMember(dest => dest.Images,
                opt => opt.MapFrom(src => src.Images));

        CreateMap<ReqImageDTO, Image>();
      
        CreateMap<Image, ResImageDto>()
            .ForMember(dest => dest.ImageId, opt => opt.MapFrom(src => src.Id));// Maps the 'Id' property from Image to the 'ImageId' property in ResImageDto.




    }
}