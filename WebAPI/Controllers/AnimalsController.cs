using Application.Animals.Commands;
using Application.Animals.Queries;
using Application.Core;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;


namespace WebAPI.Controllers;

public class AnimalsController(IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<ResAnimalDto>>> GetAnimals()
    {
        var result = await Mediator.Send(new GetAnimalList.Query());

        var animalDtoList = mapper.Map<List<ResAnimalDto>>(result);

        return HandleResult(Result<List<ResAnimalDto>>.Success(animalDtoList));
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateAnimal([FromBody] ReqAnimalDto reqAnimalDto)
        {
            var animal = mapper.Map<Animal>(reqAnimalDto);
                
            return HandleResult(await Mediator.Send(new CreateAnimal.Command() {Animal = animal}));
        }
}
