using Application.Animals.Commands;
using Application.Animals.DTOs;
using Application.Animals.Queries;
using Domain;
using Microsoft.AspNetCore.Mvc;


namespace WebAPI.Controllers;

    public class AnimalsController : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<List<Animal>>> GetAnimals()
        {
            return await Mediator.Send(new GetAnimalList.Query());
        }

        [HttpPost]
        public async Task<ActionResult<string>> CreateAnimal(AnimalDto animalDto)
        {
            return HandleResult(await Mediator.Send(new CreateAnimal.Command { AnimalDto = animalDto }));
        }
}
