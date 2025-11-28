using Domain;

namespace Application.Interfaces;

/// <summary>
/// Service interface for fetching breed data from external APIs
/// </summary>
public interface IExternalBreedService
{
    /// <summary>
    /// Fetches all dog breeds from external API and maps to domain entities
    /// </summary>
    /// <returns>List of Breed domain entities</returns>
    /// <exception cref="HttpRequestException">When the API request fails</exception>
    Task<List<Breed>> FetchBreedsAsync();
}
