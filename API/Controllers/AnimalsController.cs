using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Controllers
{
    public class AnimalsController(AppDbContext dbContext) : BaseApiController
    {

        [HttpGet]

        public async Task<ActionResult<List<Animal>>> GetAnimals()
        {
            return await dbContext.Animals.ToListAsync();
        }

        [HttpGet("{id:guid}")]

        public async Task<ActionResult<Animal>> GetAnimalDetails(string id)
        {
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

            return Ok(animal);

        }
    }
}
