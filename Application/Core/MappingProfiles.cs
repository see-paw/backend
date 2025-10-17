using Application.Animals.DTOs;
using AutoMapper;
using Domain;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Animal, Animal>();
        CreateMap<AnimalDto, Animal>();
    }
}