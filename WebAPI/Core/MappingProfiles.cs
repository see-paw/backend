using AutoMapper;
using Domain;
using WebAPI.DTOs;

namespace WebAPI.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ReqCreateAnimalDto, Animal>()
            .ForMember(dest => dest.Breed, opt => opt.Ignore()); // FK

        CreateMap<Breed, ResBreedDto>();

        CreateMap<Animal, ResAnimalDto>()
            .ForMember(dest => dest.AnimalId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Age, 
                opt => opt.MapFrom(src => DateTime.Today.Year - src.BirthDate.Year))
            .ForMember(dest => dest.Breed,
                opt => opt.MapFrom(src => src.Breed));


    }
}