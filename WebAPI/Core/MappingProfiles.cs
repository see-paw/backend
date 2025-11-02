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

        CreateMap<ReqCreateImageDto, Image>(MemberList.Source)
            .ForSourceMember(src => src.File, opt => opt.DoNotValidate());
        
        CreateMap<ReqImageDto, Image>(MemberList.Source)
            .ForMember(dest => dest.IsPrincipal, opt => opt.MapFrom(src => false))
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
                        Description = i.Description ?? string.Empty,
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

        // Favorite Animal mappings
        CreateMap<Animal, ResFavoriteAnimalDto>()
            .ForMember(dest => dest.Breed, opt => opt.MapFrom(src => src.Breed.Name))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.BirthDate)))
            .ForMember(dest => dest.PrincipalImageUrl, opt => opt.MapFrom(src =>
                src.Images.FirstOrDefault(i => i.IsPrincipal)!.Url))
            .ForMember(dest => dest.ShelterName, opt => opt.MapFrom(src => src.Shelter.Name));

        CreateMap<ReqUserProfileDto, User>();
        
        //  OwnershipRequests mapping
        CreateMap<OwnershipRequest, ResUserOwnershipsDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.AnimalId, o => o.MapFrom(s => s.Animal.Id))
            .ForMember(d => d.AnimalName, o => o.MapFrom(s => s.Animal.Name))
            .ForMember(d => d.AnimalState, o => o.MapFrom(s => s.Animal.AnimalState))
            .ForMember(d => d.Image, o => o.MapFrom(s => s.Animal.Images.FirstOrDefault(i => i.IsPrincipal)))
            .ForMember(d => d.Amount, o => o.MapFrom(s => s.Animal.Cost))
            .ForMember(d => d.OwnershipStatus, o => o.MapFrom(s => s.Status))
            .ForMember(d => d.RequestInfo, o => o.MapFrom(s => s.RequestInfo))
            .ForMember(d => d.RequestedAt, o => o.MapFrom(s => s.RequestedAt))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt));
           

        //  Animals owned mapping
        CreateMap<Animal, ResUserOwnershipsDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.AnimalId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.AnimalName, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.AnimalState, o => o.MapFrom(s => s.AnimalState))
            .ForMember(d => d.Image, o => o.MapFrom(s => s.Images.FirstOrDefault(i => i.IsPrincipal)))
            .ForMember(d => d.Amount, o => o.MapFrom(s => s.Cost))
            .ForMember(d => d.RequestedAt, o => o.MapFrom(s => s.OwnershipStartDate ?? DateTime.UtcNow))
            .ForMember(d => d.ApprovedAt, o => o.MapFrom(s => s.OwnershipStartDate))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt));
    }

    private static int CalculateAge(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - birthDate.Year;
            if (today < birthDate.AddYears(age)) age--;
            return age;
        }
}


