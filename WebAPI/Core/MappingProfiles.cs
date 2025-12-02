using Application.Activities.Commands;
using Application.Animals.Filters;
using Application.Auth.Commands;
using Application.Scheduling;
using Application.Users.Queries;

using AutoMapper;

using Domain;
using Domain.Enums;

using WebAPI.DTOs.Activities;
using WebAPI.DTOs.Animals;
using WebAPI.DTOs.AnimalSchedule;
using WebAPI.DTOs.Auth;
using WebAPI.DTOs.Breeds;
using WebAPI.DTOs.Favorites;
using WebAPI.DTOs.Fostering;
using WebAPI.DTOs.Images;
using WebAPI.DTOs.Notifications;
using WebAPI.DTOs.Ownership;
using WebAPI.DTOs.Shelter;
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
                opt => opt.MapFrom(src => src.Images))
            .ForMember(dest => dest.CurrentSupportValue,
            opt => opt.MapFrom(src => src.Fosterings
                .Where(f => f.Status == FosteringStatus.Active)
                .Sum(f => f.Amount)));


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

        // Activity mappings
        CreateMap<Activity, ResActivityDto>()
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

        // Notifications
        CreateMap<Notification, ResNotificationDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        // Mapeia o agendamento semanal completo
        CreateMap<AnimalWeeklySchedule, AnimalWeeklyScheduleDto>()
            .ForMember(dest => dest.Animal, opt => opt.MapFrom(src => src.Animal))
            .ForMember(dest => dest.Shelter, opt => opt.MapFrom(src => src.Shelter))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(d => d.Days, opt => opt.MapFrom(src => src.WeekSchedule));

        // Mapeia cada dia
        CreateMap<DailySchedule, DailyScheduleDto>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToString("yyyy-MM-dd")))
            .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.AvailableSlots))
            .ForMember(dest => dest.ReservedSlots, opt => opt.MapFrom(src => src.ReservedSlots))
            .ForMember(dest => dest.UnavailableSlots, opt => opt.MapFrom(src => src.UnavailableSlots));

        // Blocos disponíveis
        CreateMap<TimeBlock, SlotDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Start, opt => opt.MapFrom(src => src.Start.ToString(@"hh\:mm")))
            .ForMember(dest => dest.End, opt => opt.MapFrom(src => src.End.ToString(@"hh\:mm")));

        CreateMap<Slot, SlotDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Start, opt => opt.MapFrom(src => src.StartDateTime.ToString("HH:mm")))
            .ForMember(dest => dest.End, opt => opt.MapFrom(src => src.EndDateTime.ToString("HH:mm")));

        // Slots de atividade (reservas)
        CreateMap<ActivitySlot, ActivitySlotDto>()
            .IncludeBase<Slot, SlotDto>()
            .ForMember(dest => dest.ReservedBy,
                opt => opt.MapFrom(src => src.Activity.User.UserName))
            .ForMember(dest => dest.IsOwnReservation, opt => opt.Ignore());

        // Slots de indisponibilidade do abrigo
        CreateMap<ShelterUnavailabilitySlot, ShelterUnavailabilitySlotDto>()
            .IncludeBase<Slot, SlotDto>()
            .ForMember(d => d.Reason, o => o.MapFrom(s => s.Reason));

        CreateMap<Shelter, ResShelterDto>();

        // User Registration mapping
        CreateMap<ReqRegisterUserDto, User>()
            // Set the Identity username to the user's email (login is based on UserName).
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            // Automatically assign the account creation timestamp at registration time.
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            // Ignore collections and UpdatedAt
            .ForMember(dest => dest.Shelter, opt => opt.Ignore())
            .ForMember(dest => dest.Favorites, opt => opt.Ignore())
            .ForMember(dest => dest.OwnedAnimals, opt => opt.Ignore())
            .ForMember(dest => dest.Fosterings, opt => opt.Ignore())
            .ForMember(dest => dest.OwnershipRequests, opt => opt.Ignore())
            .ForMember(dest => dest.Activities, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<ReqRegisterUserDto, Register.Command>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.SelectedRole, opt => opt.MapFrom(src => src.SelectedRole))
            // if role == AdminCAA
            .ForMember(dest => dest.ShelterName, opt => opt.MapFrom(src => src.ShelterName))
            .ForMember(dest => dest.ShelterStreet, opt => opt.MapFrom(src => src.ShelterStreet))
            .ForMember(dest => dest.ShelterCity, opt => opt.MapFrom(src => src.ShelterCity))
            .ForMember(dest => dest.ShelterPostalCode, opt => opt.MapFrom(src => src.ShelterPostalCode))
            .ForMember(dest => dest.ShelterPhone, opt => opt.MapFrom(src => src.ShelterPhone))
            .ForMember(dest => dest.ShelterNIF, opt => opt.MapFrom(src => src.ShelterNIF))
            .ForMember(dest => dest.ShelterOpeningTime, opt => opt.MapFrom(src => src.ShelterOpeningTime))
            .ForMember(dest => dest.ShelterClosingTime, opt => opt.MapFrom(src => src.ShelterClosingTime));

        CreateMap<User, ResRegisterUserDto>()
            .ForMember(dest => dest.Role, opt => opt.Ignore());

        CreateMap<Shelter, ResRegisterShelterDto>()
            .ForMember(dest => dest.OpeningTime, opt => opt.MapFrom(src => src.OpeningTime.ToString("HH:mm")))
            .ForMember(dest => dest.ClosingTime, opt => opt.MapFrom(src => src.ClosingTime.ToString("HH:mm")));

        CreateMap<GetCurrentUser.UserInfo, ResCurrentUserDto>();

        // Mapping de CreateFosteringActivityResult para ResActivityFosteringDto
        CreateMap<CreateFosteringActivity.CreateFosteringActivityResult, ResActivityFosteringDto>()
            .ForMember(dest => dest.ActivitySlotId, opt => opt.MapFrom(src => src.ActivitySlot.Id))
            .ForMember(dest => dest.ActivityId, opt => opt.MapFrom(src => src.Activity.Id))
            .ForMember(dest => dest.StartDateTime, opt => opt.MapFrom(src => src.ActivitySlot.StartDateTime))
            .ForMember(dest => dest.EndDateTime, opt => opt.MapFrom(src => src.ActivitySlot.EndDateTime))
            .ForMember(dest => dest.Animal, opt => opt.MapFrom(src => src.Animal))
            .ForMember(dest => dest.Shelter, opt => opt.MapFrom(src => src.Shelter))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(_ => "Visit scheduled successfully"));

        // Mapping de Animal para AnimalVisitInfoDto
        CreateMap<Animal, AnimalVisitInfoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.PrincipalImageUrl, opt => opt.MapFrom(src =>
                src.Images.FirstOrDefault(i => i.IsPrincipal) != null
                    ? src.Images.FirstOrDefault(i => i.IsPrincipal)!.Url
                    : null));

        // Mapping de Shelter para ShelterVisitInfoDto
        CreateMap<Shelter, ShelterVisitInfoDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src =>
                $"{src.Street}, {src.City}, {src.PostalCode}"))
            .ForMember(dest => dest.OpeningTime, opt => opt.MapFrom(src => src.OpeningTime))
            .ForMember(dest => dest.ClosingTime, opt => opt.MapFrom(src => src.ClosingTime));


        // Mapping para Cancel Foster Activity
        CreateMap<CancelFosteringActivity.CancelFosteringActivityResult, ResCancelActivityFosteringDto>()
            .ForMember(dest => dest.ActivityId, opt => opt.MapFrom(src => src.ActivityId))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message));

        CreateMap<AnimalFilterDto, AnimalFilterModel>()
            .ConvertUsing<AnimalFilterDtoConverter>();

        // Mapping for FosteringVisitDto
        CreateMap<Activity, ResFosteringVisitDto>()
            .ForMember(d => d.ActivityId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.AnimalName, o => o.MapFrom(s => s.Animal.Name))
            .ForMember(d => d.AnimalPrincipalImageUrl, o => o.MapFrom(s =>
                s.Animal.Images.FirstOrDefault(img => img.IsPrincipal)!.Url))
            .ForMember(d => d.BreedName, o => o.MapFrom(s => s.Animal.Breed.Name))
            .ForMember(d => d.AnimalAge, o => o.MapFrom(s =>
                CalculateAge(s.Animal.BirthDate)))
            .ForMember(d => d.ShelterName, o => o.MapFrom(s => s.Animal.Shelter.Name))
            .ForMember(d => d.ShelterAddress, o => o.MapFrom(s =>
                $"{s.Animal.Shelter.Street}, {s.Animal.Shelter.PostalCode} {s.Animal.Shelter.City}"))
            .ForMember(d => d.ShelterOpeningTime, o => o.MapFrom(s => s.Animal.Shelter.OpeningTime))
            .ForMember(d => d.ShelterClosingTime, o => o.MapFrom(s => s.Animal.Shelter.ClosingTime))
            .ForMember(d => d.VisitStartDateTime, o => o.MapFrom(s => s.Slot!.StartDateTime))
            .ForMember(d => d.VisitEndDateTime, o => o.MapFrom(s => s.Slot!.EndDateTime))
            .ForMember(d => d.VisitDate, o => o.MapFrom(s =>
                DateOnly.FromDateTime(s.Slot!.StartDateTime)));

        // Mapping from Shelter to ResShelterInfoDto
        CreateMap<Shelter, ResShelterInfoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Street))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Nif, opt => opt.MapFrom(src => src.NIF))
            .ForMember(dest => dest.OpeningTime, opt =>
                opt.MapFrom(src => src.OpeningTime.ToString("HH:mm")))
            .ForMember(dest => dest.ClosingTime, opt =>
                opt.MapFrom(src => src.ClosingTime.ToString("HH:mm")));
    }

    private static int CalculateAge(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - birthDate.Year;
        if (today < birthDate.AddYears(age)) age--;
        return age;
    }
}
