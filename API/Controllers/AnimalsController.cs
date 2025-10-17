using API.Core;
using API.DTOs;
using AutoMapper;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Controllers
{
    /// <summary>
    /// Controller responsible for managing animal-related operations.
    /// </summary>
    /// <remarks>
    /// This controller interacts directly with the database through <see cref="AppDbContext"/>,
    /// applies business logic and uses <see cref="AutoMapper"/> to map DTOs to entities.
    /// </remarks>
    
    public class AnimalsController : BaseApiController
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimalsController"/> class.
        /// </summary>
        /// <param name="context">Database context used to access and manage animal data.</param>
        /// <param name="mapper">AutoMapper instance used for DTO-to-entity conversions.</param>
        public AnimalsController(AppDbContext context, IMapper mapper) : base(context, mapper) { }
        

        /// <summary>
        /// Retrieves a paginated list of available or partially fostered animals.
        /// </summary>
        /// <param name="pageNumber">Page number for pagination (defaults to 1).</param>
        /// <returns>
        /// A paginated list of animals, ordered alphabetically by name.
        /// Returns <see cref="NotFoundResult"/> if no animals are found.
        /// </returns>
        /// <response code="200">Returns the list of animals.</response>
        /// <response code="404">No animals found.</response>
        [HttpGet]
        public async Task<ActionResult<PagedList<Animal>>> GetAnimals([FromQuery] int pageNumber = 1)
        {
            if (pageNumber < 1)
            {
                return BadRequest("Page number must be 1 or greater");
            }

            const int pageSize = 20; // Default page size

            var query = _dbContext.Animals
                .Where(a => a.AnimalState == AnimalState.Available
                         || a.AnimalState == AnimalState.PartiallyFostered)
                .OrderBy(a => a.Name)
                .AsQueryable();

            var pagedList = await PagedList<Animal>.CreateAsync(query, pageNumber, pageSize);

            if (pagedList == null || pagedList.Count == 0)
                return NotFound("No animals found");

            return Ok(pagedList);
        }

        /// <summary>
        /// Creates a new animal record associated with a specific shelter.
        /// </summary>
        /// <param name="animalDTO">The data transfer object containing animal details.</param>
        /// <returns>
        /// The unique identifier (GUID) of the newly created animal.
        /// </returns>
        /// <response code="201">Animal successfully created and persisted in the database.</response>
        /// <response code="400">Validation error or failed to create the animal.</response>
        /// <response code="401">Invalid or missing shelter token.</response>
        /// <response code="404">Shelter not found.</response>
        [HttpPost]
        public async Task<ActionResult<string>> CreateAnimal([FromBody] CreateAnimalDTO animalDTO)
        {
            //apagar depois, temporário enquanto não temos autenticação
            var shelterId = "22222222-2222-2222-2222-222222222222"; // mudar para testar
            // var shelterId = User.FindFirst("shelterId")?.Value; // depois com JWT

            if (string.IsNullOrEmpty(shelterId))
                return Unauthorized("Invalid shelter token");

            // Check if the shelter exists in the database
            var shelterExists = await _dbContext.Shelters
                .AnyAsync(s => s.ShelterId == shelterId);

            if (!shelterExists)
                return NotFound("Shelter not found");

            // Create animal entity from the DTO
            var animal = _mapper.Map<Animal>(animalDTO);
            animal.AnimalState = AnimalState.Available;
            animal.CreatedAt = DateTime.UtcNow;
            animal.UpdatedAt = DateTime.UtcNow;
            animal.ShelterId = shelterId;

            // Add animal to the database
            _dbContext.Animals.Add(animal);

            var result = await _dbContext.SaveChangesAsync() > 0;

            if (!result)
                return BadRequest("Failed to create the animal");

            // Return the ID of the newly created animal
            return Created("", animal.AnimalId);
        }
    }
}
