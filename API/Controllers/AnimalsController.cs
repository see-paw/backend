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
    public class AnimalsController(AppDbContext dbContext, IMapper mapper) : BaseApiController
    {

        [HttpGet]

        public async Task<ActionResult<List<Animal>>> GetAnimals()
        {
            var animals = await dbContext.Animals.ToListAsync();
            var animalDTOs = mapper.Map<List<AnimalDTO>>(animals);
            return Ok(animalDTOs);
        }

        [HttpGet("{id:guid}")]

        public async Task<ActionResult<AnimalDTO>> GetAnimalDetails(string id)
        {
            var validation = ValidateId(id);

            if (validation != null)
            {
                return validation;
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

            var animalDto = mapper.Map<AnimalDTO>(animal);

            return Ok(animalDto);

        }
    }
}
