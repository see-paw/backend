using API.Core;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Persistence;


namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SheltersController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public SheltersController(AppDbContext context)
        {
            this.dbContext = context;
        }

        [HttpGet("{shelterId}/animals")]
        public async Task<ActionResult<PagedList<Animal>>> GetAnimalsByShelter(string shelterId, [FromQuery] int pageNumber = 1)
        {
            const int pageSize = 20; //default page size

            // Check if the shelter exists
            var shelter = await dbContext.Shelters.FindAsync(shelterId);
            if (shelter == null)
            {
                return NotFound("Shelter not found");
            }

            // Filter animals by shelter
            var query = dbContext.Animals
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