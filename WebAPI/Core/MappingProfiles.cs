using Application.Animals.Commands;
using AutoMapper;
using Domain;
using WebAPI.DTOs;

namespace WebAPI.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ReqAnimalDto, Animal>();
        CreateMap<Animal, ResAnimalDto>()
            .ForMember(dest => dest.Age, 
                opt => opt.MapFrom(src => DateTime.Today.Year - src.BirthDate.Year))
            .ForMember(dest => dest.BreedName, opt => opt.MapFrom(src => src.Breed.Name))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(x => x.Images));
        CreateMap<Image, ResImageDto>();
    }
}