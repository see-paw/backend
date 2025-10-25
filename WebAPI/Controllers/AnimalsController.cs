using Application.Animals.Commands;
using Application.Animals.Queries;
using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace WebAPI.Controllers;

public class AnimalsController(IMapper mapper, IUserAccessor userAccessor) : BaseApiController
{
    /// <summary>
    /// Retrieves a paginated list of animals that are available or partially fostered.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve. Defaults to 1.</param>
    /// <returns>
    /// A paginated <see cref="PagedList{T}"/> of <see cref="ResAnimalDto"/> objects representing the animals.
    /// Returns <c>400</c> if the page number is invalid or an appropriate error message on failure.
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<PagedList<ResAnimalDto>>> GetAnimals([FromQuery] int pageNumber = 1)
    {
        var result = await Mediator.Send(new GetAnimalList.Query
        {
            PageNumber = pageNumber
        });

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var dtoList = mapper.Map<List<ResAnimalDto>>(result.Value);

        // Create a new paginated list with the DTOs
        var dtoPagedList = new PagedList<ResAnimalDto>(
            dtoList,
            result.Value.TotalCount,
            result.Value.CurrentPage,
            result.Value.PageSize
        );

        // Return the successful paginated result
        return HandleResult(Result<PagedList<ResAnimalDto>>.Success(dtoPagedList, 200));
    }

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
        var result = await Mediator.Send(new GetAnimalDetails.Query() { Id = id });

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var animalDto = mapper.Map<ResAnimalDto>(result.Value);

        return HandleResult(Result<ResAnimalDto>.Success(animalDto, 200));
    }

    /// <summary>
    /// Creates a new animal record associated with a specific shelter.
    /// </summary>
    /// <param name="reqAnimalDto">The request DTO containing animal details and optional image data.</param>
    /// <returns>
    /// The unique identifier (<see cref="string"/>) of the created animal on success,
    /// or an error response (400, 404, 401) depending on the failure condition.
    /// </returns>
    [Authorize(Roles = "AdminCAA")]
    [HttpPost]
    public async Task<ActionResult<string>> CreateAnimal([FromBody] ReqCreateAnimalDto reqAnimalDto)
    {

        var user = await userAccessor.GetUserAsync();
        var shelterId = user.ShelterId;


        if (string.IsNullOrEmpty(shelterId))
            return Unauthorized("Invalid shelter token");

        // Map the validated DTO to the domain entity
        var animal = mapper.Map<Animal>(reqAnimalDto);

        // Build the command to send to the Application layer
        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelterId
        };

        // Centralized result handling (200, 400, 404, etc.)
        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Updates an existing animal record belonging to the authenticated shelter.
    /// </summary>
    /// <para>
    /// The animal identifier (<paramref name="id"/>) is obtained from the route, and the shelter context 
    /// is derived from the authenticated user’s token.
    /// </para>
    /// <param name="id">The unique identifier of the animal to be edited.</param>
    /// <param name="reqEditAnimalDto">
    /// A <see cref="ReqEditAnimalDto"/> object containing the updated animal data received from the client.
    /// </param>
    /// <returns>
    /// The updated animal record on success, or an appropriate error response on failure.
    /// </returns>
    [Authorize(Roles = "AdminCAA")]
    [HttpPut("{id}")]
    public async Task<ActionResult> EditAnimal(string id, [FromBody] ReqEditAnimalDto reqEditAnimalDto)
    {
        // Retrieve the authenticated user and shelter context
        var user = await userAccessor.GetUserAsync();
        var shelterId = user.ShelterId;

        // Ensure the shelter context is valid
        if (string.IsNullOrEmpty(shelterId))
            return Unauthorized("Invalid shelter token");

        // Map the incoming DTO to the domain entity
        var animal = mapper.Map<Animal>(reqEditAnimalDto);

        // Create and send the command through the MediatR pipeline
        var command = new EditAnimal.Command
        {
            AnimalId = id,
            Animal = animal,
            ShelterId = shelterId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var animalDto = mapper.Map<ResAnimalDto>(result.Value);

        // Execute the command and return a standardized ActionResult
        return HandleResult(Result<ResAnimalDto>.Success(animalDto, 200));
        
    }

    /// <summary>
    /// Deletes an existing animal record associated with the authenticated shelter.
    /// </summary>
    /// <para>
    /// The animal identifier (<paramref name="id"/>) is obtained from the route, and the shelter context 
    /// is derived from the authenticated user’s token.
    /// </para>
    /// <param name="id">The unique identifier of the animal to be edited.</param>
    /// </param>
    /// <returns>
    /// The deleted animal record on success, or an appropriate error response on failure.
    /// </returns>
    [Authorize(Roles = "AdminCAA")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ResAnimalDto>> DeleteAnimal(string id)
    {
        // Retrieve the authenticated user and shelter context
        var user = await userAccessor.GetUserAsync();
        var shelterId = user.ShelterId;

        // Ensure the shelter context is valid
        if (string.IsNullOrEmpty(shelterId))
            return Unauthorized("Invalid shelter token");

        // Create and send the command through the MediatR pipeline
        var command = new DeleteAnimal.Command
        {
            AnimalId = id,
            ShelterId = shelterId
        };

        // Execute the command
        var result = await Mediator.Send(command);

        if (!result.IsSuccess || result.Value == null)
            return HandleResult(result);

        // Map the deleted Animal entity to a response DTO
        var animalDto = mapper.Map<ResAnimalDto>(result.Value);

        // Return standardized ActionResult with DTO and HTTP 200 status
        return HandleResult(Result<ResAnimalDto>.Success(animalDto, 200));
    }

}