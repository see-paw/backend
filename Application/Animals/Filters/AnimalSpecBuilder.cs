using Application.Animals.Filters.Specs;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.Services;

namespace Application.Animals.Filters;

/// <summary>
/// Builder responsible for creating specifications from filter criteria.
/// </summary>
public class AnimalSpecBuilder(AnimalDomainService animalDomainService)
{
    /// <summary>
    /// Builds a list of specifications based on the provided filter model.
    /// Only creates specifications for non-null/non-empty filter values.
    /// </summary>
    /// <param name="filter">The filter criteria</param>
    /// <returns>List of specifications to apply</returns>
    public List<ISpecification<Animal>> Build(AnimalFilterModel filter)
    {
        var specs = new List<ISpecification<Animal>>();

        if (filter.Age.HasValue)
            specs.Add(new AgeSpec(animalDomainService) { Age = filter.Age.Value });

        if (!string.IsNullOrWhiteSpace(filter.Breed))
            specs.Add(new BreedSpec { BreedName = filter.Breed });

        if (!string.IsNullOrWhiteSpace(filter.Name))
            specs.Add(new NameSpec { Name = filter.Name });

        if (!string.IsNullOrWhiteSpace(filter.Sex) && Enum.TryParse<SexType>(filter.Sex, true, out var sex))
            specs.Add(new SexSpec { Sex = sex });

        if (!string.IsNullOrWhiteSpace(filter.ShelterName))
            specs.Add(new ShelterNameSpec { ShelterName = filter.ShelterName });

        if (!string.IsNullOrWhiteSpace(filter.Size) && Enum.TryParse<SizeType>(filter.Size, true, out var size))
            specs.Add(new SizeSpec { Size = size });

        if (!string.IsNullOrWhiteSpace(filter.Species) && Enum.TryParse<Species>(filter.Species, true, out var species))
            specs.Add(new SpeciesSpec { Species = species });

        return specs;
    }
}