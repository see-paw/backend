using Application.Animals.Commands;
using Application.Animals.Queries;
using Application.Core;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;

namespace WebAPI.Controllers;

/// <summary>
/// Controller responsible for managing animal-related operations.
/// </summary>
/// <remarks>
/// Provides endpoints for retrieving, creating, and viewing details of animals.
/// Utilizes the <see cref="IMediator"/> pattern to delegate business logic to the application layer
/// and AutoMapper for mapping between domain entities and DTOs.
/// </remarks>
public class AnimalsController(IMapper mapper) : BaseApiController
{
    /// <summary>
    /// Retrieves a list of all animals available in the system.
    /// </summary>
    /// <returns>
    /// A list of <see cref="ResAnimalDto"/> objects representing the animals.
    /// </returns>
    /// <remarks>
    /// Sends a <see cref="GetAnimalList.Query"/> request through MediatR to fetch all animals,
    /// maps the result to DTOs using AutoMapper, and returns the list in a standardized API response.
    /// </remarks>
    //[HttpGet]
    //public async Task<ActionResult<List<ResAnimalDto>>> GetAnimals()
    //{
    //    var animalList = await Mediator.Send(new GetAnimalList.Query());

    //    var animalDtoList = mapper.Map<List<ResAnimalDto>>(animalList);

    //    return HandleResult(Result<List<ResAnimalDto>>.Success(animalDtoList, 200));
    //}

    /// <summary>
    /// Retrieves detailed information about a specific animal by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the animal.</param>
    /// <returns>
    /// A <see cref="ResAnimalDto"/> containing the animal details if found; otherwise, an appropriate error response.
    /// </returns>
    /// <remarks>
    /// Sends a <see cref="GetAnimalDetails.Query"/> through MediatR to fetch the animal data.
    /// If the animal is not found or unavailable, returns a standardized error response.
    /// Successfully retrieved entities are mapped to <see cref="ResAnimalDto"/> using AutoMapper.
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<ActionResult<ResAnimalDto>> GetAnimalDetails(string id)
    {
        var result = await Mediator.Send(new GetAnimalDetails.Query() {Id = id});

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var animalDto = mapper.Map<ResAnimalDto>(result.Value);

        return HandleResult(Result<ResAnimalDto>.Success(animalDto, 200));
    }

    //[HttpPost]
    //public async Task<ActionResult<string>> CreateAnimal([FromBody] ReqAnimalDto reqAnimalDto)
    //{
    //    var animal = mapper.Map<Animal>(reqAnimalDto);

    //    animal.Images.Add(new Image()
    //    {
    //        IsPrincipal = true,
    //        Description = reqAnimalDto.MainImageDesc,
    //        Url = reqAnimalDto.MainImageUrl
    //    });
            
    //    return HandleResult(await Mediator.Send(new CreateAnimal.Command() {Animal = animal}));
    //}
}
