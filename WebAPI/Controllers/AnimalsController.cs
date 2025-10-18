using Application.Animals.Commands;
using Application.Animals.Queries;
using Application.Core;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Core;

namespace WebAPI.Controllers;


public class AnimalsController(IMapper mapper) : BaseApiController
{

    
    [HttpGet]
    public async Task<ActionResult<PagedList<ResAnimalDto>>> GetAnimals([FromQuery] int pageNumber = 1)
    {
        if (pageNumber < 1)
            return BadRequest("Page number must be 1 or greater.");

        const int pageSize = 20;

        var result = await Mediator.Send(new GetAnimalList.Query
        {
            PageNumber = pageNumber
        });

        if (!result.IsSuccess)
            return HandleResult(result);


        //  Map the list of Animal entities to ResGetAnimalsDto (for response)
        var dtoList = mapper.Map<List<ResAnimalDto>>(result.Value);

        // Create the paginated list with DTOs
        var dtoPagedList = new PagedList<ResAnimalDto>(
            dtoList,
            result.Value.TotalCount,
            result.Value.CurrentPage,
            result.Value.PageSize
        );

        return HandleResult(Result<PagedList<ResAnimalDto>>.Success(dtoPagedList));
    }


    [HttpPost]
    public async Task<ActionResult<string>> CreateAnimal([FromBody] ReqCreateAnimalDto reqAnimalDto)
    {
        //!!!Temporary shelter ID (replace when JWT auth is ready)
        var shelterId = "22222222-2222-2222-2222-222222222222";

        if (string.IsNullOrEmpty(shelterId))
            return Unauthorized("Invalid shelter token");

        // Map the validated DTO to the domain entity
        var animal = mapper.Map<Animal>(reqAnimalDto);
       
        // Send command to Application layer 
        var command = new CreateAnimal.Command { Animal = animal, ShelterId = shelterId };

        //Centralized result handling (200, 400, 404, etc.)
        return HandleResult(await Mediator.Send(command));
    }



}
