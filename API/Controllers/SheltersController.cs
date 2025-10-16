using API.Core;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace API.Controllers
{

    /// <summary>
    /// Controller responsible for managing shelter-related operations.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints to query shelters and their related animals.
    /// It interacts directly with the database through <see cref="AppDbContext"/>.
    /// </remarks>
    public class SheltersController : BaseApiController
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SheltersController"/> class.
        /// </summary>
        /// <param name="context">Database context used to access shelter and animal data.</param>
        public SheltersController(AppDbContext dbContext) : base(dbContext) { }

        /// <summary>
        /// Retrieves all animals that belong to a specific shelter, using pagination.
        /// </summary>
        /// <param name="shelterId">The unique identifier (GUID) of the shelter.</param>
        /// <param name="pageNumber">The page number for pagination (defaults to 1).</param>
        /// <returns>
        /// A paginated list of animals belonging to the specified shelter.
        /// Returns <see cref="NotFoundResult"/> if the shelter or animals are not found.
        /// </returns>
        /// <response code="200">Returns the list of animals for the shelter.</response>
        /// <response code="404">Shelter not found or no animals associated with it.</response>
        [HttpGet("{shelterId}/animals")]
        public async Task<ActionResult<PagedList<Animal>>> GetAnimalsByShelter(string shelterId, [FromQuery] int pageNumber = 1)
        {
            if (pageNumber < 1)
            {
                return BadRequest("Page number must be 1 or greater");
            }

            const int pageSize = 20; // Default page size

            // Check if the shelter exists
            var shelter = await _dbContext.Shelters.FindAsync(shelterId);
            if (shelter == null)
            {
                return NotFound("Shelter not found");
            }

            // Filter animals by shelter
            var query = _dbContext.Animals
                .Where(a => a.ShelterId == shelterId)
                .OrderBy(a => a.Name)
                .AsQueryable();

            var pagedList = await PagedList<Animal>.CreateAsync(query, pageNumber, pageSize);

            if (pagedList == null || pagedList.Count == 0)
                return NotFound("No animals found for this shelter");

            return Ok(pagedList);
        }
    }
}
