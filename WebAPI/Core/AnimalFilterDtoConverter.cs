using Application.Animals.Filters;
using AutoMapper;
using WebAPI.DTOs.Animals;

namespace WebAPI.Core;

/// <summary>
/// AutoMapper converter that maps AnimalFilterDto to AnimalFilterModel.
/// Validates and sanitizes input to prevent invalid enum values.
/// </summary>
public class AnimalFilterDtoConverter : ITypeConverter<AnimalFilterDto, AnimalFilterModel>
{
    /// <summary>
    /// Converts AnimalFilterDto to AnimalFilterModel with input validation.
    /// Invalid enum values are ignored rather than throwing exceptions.
    /// </summary>
    public AnimalFilterModel Convert(
        AnimalFilterDto source, 
        AnimalFilterModel destination,
        ResolutionContext context)
    {
        return new AnimalFilterModel
        {
            Age = source.Age,
            Breed = source.Breed?.Trim(),
            Name = source.Name?.Trim(),
            Sex = source.Sex?.Trim(),
            ShelterName = source.ShelterName?.Trim(),
            Size = source.Size?.Trim(),
            Species = source.Species?.Trim()
        };
    }
}