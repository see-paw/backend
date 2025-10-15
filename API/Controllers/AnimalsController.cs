using API.DTOs;
using API.Validators;
using AutoMapper;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Controllers
{
    /// <summary>
    /// Handles API requests related to animal data retrieval.
    /// </summary>
    /// <remarks>
    /// The <see cref="AnimalsController"/> provides endpoints for accessing animal information
    /// stored in the application database.  
    /// 
    /// It supports two main operations:
    /// <list type="bullet">
    /// <item><description><c>GET /api/animals</c> — Retrieves a list of all animals.</description></item>
    /// <item><description><c>GET /api/animals/{id}</c> — Retrieves detailed information about a specific animal by ID.</description></item>
    /// </list>
    /// 
    /// The controller ensures data validation, applies visibility rules based on <see cref="AnimalState"/>,
    /// and uses <see cref="IMapper"/> to convert domain entities (<see cref="Domain.Animal"/>) into <see cref="API.DTOs.AnimalDto"/> objects.
    /// </remarks>

    public class AnimalsController(AppDbContext dbContext, IMapper mapper) : BaseApiController
    {
        /// <summary>
        /// Retrieves a list of all animals from the database.
        /// </summary>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a collection of <see cref="AnimalDto"/> objects
        /// representing all animals managed by the system.
        /// </returns>
        /// <remarks>
        /// This endpoint returns all animals stored in the database, regardless of their current <see cref="AnimalState"/>.  
        /// The results are automatically mapped from <see cref="Domain.Animal"/> entities to <see cref="API.DTOs.AnimalDto"/> objects
        /// using <see cref="IMapper"/> before being sent in the response.
        /// </remarks>

        [HttpGet]

        public async Task<ActionResult<List<Animal>>> GetAnimals()
        {
            var animals = await dbContext.Animals.ToListAsync();
            var animalDtOs = mapper.Map<List<AnimalDto>>(animals);
            return Ok(animalDtOs);
        }

        /// <summary>
        /// Retrieves detailed information about a specific animal identified by its unique ID.
        /// </summary>
        /// <param name="id">The unique identifier (<see cref="Guid"/>) of the animal to retrieve.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing the <see cref="AnimalDto"/> data
        /// if the animal exists and is visible, or an appropriate HTTP error response otherwise.
        /// </returns>
        /// <remarks>
        /// This endpoint performs several validation and filtering steps:
        /// <list type="number">
        /// <item><description>Validates that the provided <paramref name="id"/> is a valid <see cref="Guid"/>.</description></item>
        /// <item><description>Returns <see cref="BadRequestObjectResult"/> if the ID is invalid.</description></item>
        /// <item><description>Returns <see cref="NotFoundObjectResult"/> if the animal does not exist in the database.</description></item>
        /// <item><description>Returns <see cref="NotFoundObjectResult"/> if the animal’s <see cref="AnimalState"/> is not visible
        /// (only <see cref="AnimalState.Available"/> and <see cref="AnimalState.PartiallyFostered"/> are visible).</description></item>
        /// <item><description>Returns <see cref="OkObjectResult"/> with an <see cref="AnimalDto"/> when the animal is found and valid.</description></item>
        /// </list>
        /// The method uses <see cref="GuidStringValidator"/> for input validation and <see cref="IMapper"/> for mapping domain entities
        /// to data transfer objects.
        /// </remarks>

        [HttpGet("{id}")]

        public async Task<ActionResult<AnimalDto>> GetAnimalDetails(string id)
        {
            var validator = new GuidStringValidator();
            var result = await validator.ValidateAsync(id);

            if (!result.IsValid)
            {
                return BadRequest(result.Errors.Select(error => error.ErrorMessage));
            }

            var animal = await dbContext.FindAsync<Animal>(id);

            if (animal == null)
            {
                return NotFound("Animal not Found");
            }

            var visibleStates = new[] { AnimalState.Available, AnimalState.PartiallyFostered };

            if (!visibleStates.Contains(animal.AnimalState))
            {
                return NotFound("Animal not Available");
            }

            var animalDto = mapper.Map<AnimalDto>(animal);

            return Ok(animalDto);

        }
    }
}
