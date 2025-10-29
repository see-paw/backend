﻿using AutoMapper;
using Domain;
using WebAPI.DTOs;
using WebAPI.DTOs.Animals;
using WebAPI.DTOs.Breeds;
using WebAPI.DTOs.Fostering;
using WebAPI.DTOs.Images;
using WebAPI.DTOs.Ownership;
using WebAPI.DTOs.User;

namespace WebAPI.Core;

/// <summary>
/// Defines AutoMapper configuration profiles for mapping between entities and DTOs.
/// </summary>
/// <remarks>
/// Centralizes all object-to-object mappings used across the application,  
/// ensuring consistent data transformation between the domain and API layers.
/// </remarks>
public class MappingProfiles : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingProfiles"/> class  
    /// and registers all entity-to-DTO and DTO-to-entity mappings.
    /// </summary>
    public MappingProfiles()
    {
        CreateMap<ReqCreateAnimalDto, Animal>(MemberList.Source)
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForSourceMember(src => src.Images, opt => opt.DoNotValidate());
        
        CreateMap<Breed, ResBreedDto>();

        CreateMap<Animal, ResAnimalDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Age,
                opt => opt.MapFrom(src => DateTime.Today.Year - src.BirthDate.Year))
            .ForMember(dest => dest.Breed,
                opt => opt.MapFrom(src => src.Breed))
            .ForMember(dest => dest.Images,
                opt => opt.MapFrom(src => src.Images));

        CreateMap<ReqImageDto, Image>(MemberList.Source)
            .ForSourceMember(src => src.File, opt => opt.DoNotValidate());

        CreateMap<Image, ResImageDto>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src =>
                    src.Id)); // Maps the 'Id' property from Image to the 'ImageId' property in ResImageDto.

        CreateMap<ReqEditAnimalDto, Animal>(MemberList.Source);

        //For edit Animal
        CreateMap<Animal, Animal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Fosterings, opt => opt.Ignore())
            .ForMember(dest => dest.OwnershipRequests, opt => opt.Ignore())
            .ForMember(dest => dest.Activities, opt => opt.Ignore())
            .ForMember(dest => dest.Favorites, opt => opt.Ignore())
            .ForMember(dest => dest.Owner, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.ShelterId, opt => opt.Ignore())
            .ForMember(dest => dest.Shelter, opt => opt.Ignore());
        
        CreateMap<Fostering, ResActiveFosteringDto>()
            // Flatten animal properties
            .ForMember(dest => dest.AnimalName, opt => opt.MapFrom(src => src.Animal.Name))
            .ForMember(dest => dest.AnimalAge, opt => opt.MapFrom(src => CalculateAge(src.Animal.BirthDate)))
            // Pick only principal image
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
                src.Animal.Images
                    .Where(i => i.IsPrincipal)
                    .Select(i => new ResImageDto
                    {
                        Id = i.Id,
                        Url = i.Url,
                        Description = i.Description,
                        IsPrincipal = i.IsPrincipal,
                        PublicId = i.PublicId
                    }).ToList()))
            // Map fostering-specific fields
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate));

        CreateMap<Fostering, ResCancelFosteringDto>()
            // Flatten animal properties
            .ForMember(dest => dest.AnimalName, opt => opt.MapFrom(src => src.Animal.Name))
            .ForMember(dest => dest.AnimalAge, opt => opt.MapFrom(src => CalculateAge(src.Animal.BirthDate)))
            // Map fostering-specific fields
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));


        CreateMap<User, ResUserProfileDto>();

        // OwnershipRequest mappings
        CreateMap<OwnershipRequest, ResOwnershipRequestDto>()
            .ForMember(dest => dest.AnimalName, opt => opt.MapFrom(src => src.Animal.Name))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name));
        
        CreateMap<ReqUserProfileDto, User>(MemberList.Source);
    }

    private static int CalculateAge(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - birthDate.Year;
            if (today < birthDate.AddYears(age)) age--;
            return age;
        }
}