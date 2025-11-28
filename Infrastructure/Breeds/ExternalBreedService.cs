using Domain;
using Application.Interfaces;


using System.Net.Http.Json;

namespace Infrastructure.Breeds;



/// <summary>
/// Service for fetching breed data from external APIs
/// </summary>
public class ExternalBreedService : IExternalBreedService
{
    private readonly HttpClient _httpClient;
    private const string DogApiBaseUrl = "https://api.thedogapi.com/v1/breeds";

    public ExternalBreedService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Fetches all dog breeds from TheDogAPI and maps to domain entities
    /// </summary>
    /// <returns>List of Breed domain entities</returns>
    /// <exception cref="HttpRequestException">When the API request fails</exception>
    public async Task<List<Breed>> FetchBreedsAsync()
    {
        try
        {
            // 1. Fetch from external API
            var response = await _httpClient.GetAsync(DogApiBaseUrl);
            response.EnsureSuccessStatusCode();

            var apiBreeds = await response.Content.ReadFromJsonAsync<List<DogApiBreed>>();

            if (apiBreeds == null || !apiBreeds.Any())
            {
                return new List<Breed>();
            }

            // 2. Map DTO → Domain Entity (Infrastructure responsibility)
            var breeds = apiBreeds.Select(api => new Breed
            {
                Id = Guid.NewGuid().ToString(),
                Name = api.Name,
                Description = api.Temperament,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            return breeds;
        }
        catch (HttpRequestException ex)
        {
            // Log error (if logger is available)
            throw new HttpRequestException($"Failed to fetch breeds from TheDogAPI: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            // Log error (if logger is available)
            throw new Exception($"Unexpected error while fetching breeds: {ex.Message}", ex);
        }
    }
}
