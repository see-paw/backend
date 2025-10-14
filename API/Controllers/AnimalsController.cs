using API.Core;
using API.DTOs;
using Application.Animals.DTOs;
using AutoMapper;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimalsController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IMapper mapper;

        public AnimalsController(AppDbContext context, IMapper mapper)
        {
            this.dbContext = context;
            this.mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<PagedList<Animal>>> GetAnimals([FromQuery] int pageNumber = 1)
        {
            const int pageSize = 20; // default page size

            var query = dbContext.Animals
                .Where(a => a.AnimalState == AnimalState.Available
                         || a.AnimalState == AnimalState.PartiallyFostered)
                .OrderBy(a => a.Name)
                .AsQueryable();

            var pagedList = await PagedList<Animal>.CreateAsync(query, pageNumber, pageSize);

            if (pagedList == null || pagedList.Count == 0)
                return NotFound("No animals found");

            return Ok(pagedList);
        }

        [HttpPost]
        public async Task<ActionResult<string>> CreateAnimal([FromBody] CreateAnimalDTO animalDTO)
        {
            //Obter o shelterId do token (simulado enquanto não há auth)
            var shelterId = "11111111-1111-1111-1111-111111111111";
            // var shelterId = User.FindFirst("shelterId")?.Value; // usar mais tarde com JWT

            if (string.IsNullOrEmpty(shelterId))
                return Unauthorized("Invalid shelter token");

            // check if shelter exists
            var shelterExists = await dbContext.Shelters
                .AnyAsync(s => s.ShelterId == shelterId);

            if (!shelterExists)
                return NotFound("Shelter not found");

            // create animal from DTO
            var animal = mapper.Map<Animal>(animalDTO);
            animal.AnimalState = AnimalState.Available;
            animal.CreatedAt = DateTime.UtcNow;
            animal.UpdatedAt = DateTime.UtcNow;
            animal.ShelterId = shelterId;

            // add animal to the database
            dbContext.Animals.Add(animal);

            var result = await dbContext.SaveChangesAsync() > 0;

            if (!result)
                return BadRequest("Failed to create the animal");

            //return the id of the created animal
            return Created("", animal.AnimalId);
        }

       
    }
}

