using Application.Animals.Commands;
using Application.Animals.Queries;
using Application.Core;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Core;

namespace WebAPI.Controllers
{
    /// <summary>
    /// API controller responsible for handling all operations related to <see cref="Animal"/> entities.
    /// Includes endpoints for retrieving paginated animal lists and creating new animals.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AnimalsController : BaseApiController
    {
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimalsController"/> class.
        /// </summary>
        /// <param name="mapper">The AutoMapper instance used for DTO-to-domain and domain-to-DTO mapping.</param>
        public AnimalsController(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paginated list of animals that are available or partially fostered.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve. Defaults to 1.</param>
        /// <returns>
        /// A paginated <see cref="PagedList{T}"/> of <see cref="ResAnimalDTO"/> objects representing the animals.
        /// Returns <c>400</c> if the page number is invalid or an appropriate error message on failure.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<PagedList<ResAnimalDTO>>> GetAnimals([FromQuery] int pageNumber = 1)
        {

            var result = await Mediator.Send(new GetAnimalList.Query
            {
                PageNumber = pageNumber
            });

            if (!result.IsSuccess)
                return HandleResult(result);

            // Map the list of Animal entities to response DTOs
            var dtoList = _mapper.Map<List<ResAnimalDTO>>(result.Value);

            // Create a new paginated list with the DTOs
            var dtoPagedList = new PagedList<ResAnimalDTO>(
                dtoList,
                result.Value.TotalCount,
                result.Value.CurrentPage,
                result.Value.PageSize
            );

            // Return the successful paginated result
            return HandleResult(Result<PagedList<ResAnimalDTO>>.Success(dtoPagedList, 200));
        }

        /// <summary>
        /// Retrieves detailed information about a specific animal by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the animal.</param>
        /// <returns>
        /// A <see cref="ResAnimalDTO"/> containing the animal details if found; otherwise, an appropriate error response.
        /// </returns>
        /// <remarks>
        /// Sends a <see cref="GetAnimalDetails.Query"/> through MediatR to fetch the animal data.
        /// If the animal is not found or unavailable, returns a standardized error response.
        /// Successfully retrieved entities are mapped to <see cref="ResAnimalDTO"/> using AutoMapper.
        /// </remarks>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResAnimalDTO>> GetAnimalDetails(string id)
        {
            var result = await Mediator.Send(new GetAnimalDetails.Query() { Id = id });

            if (!result.IsSuccess)
            {
                return HandleResult(result);
            }

            var animalDto = _mapper.Map<ResAnimalDTO>(result.Value);

            return HandleResult(Result<ResAnimalDTO>.Success(animalDto, 200));
        }

        /// <summary>
        /// Creates a new animal record associated with a specific shelter.
        /// </summary>
        /// <param name="reqAnimalDto">The request DTO containing animal details and optional image data.</param>
        /// <returns>
        /// The unique identifier (<see cref="string"/>) of the created animal on success,
        /// or an error response (400, 404, 401) depending on the failure condition.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<string>> CreateAnimal([FromBody] ReqCreateAnimalDTO reqAnimalDto)
        {
            // Temporary shelter ID (to be replaced when JWT authentication is implemented)
            var shelterId = "22222222-2222-2222-2222-222222222222";

            if (string.IsNullOrEmpty(shelterId))
                return Unauthorized("Invalid shelter token");

            // Map the validated DTO to the domain entity
            var animal = _mapper.Map<Animal>(reqAnimalDto);


            // Build the command to send to the Application layer
            var command = new CreateAnimal.Command
            {
                Animal = animal,
                ShelterId = shelterId
            };

            // Centralized result handling (200, 400, 404, etc.)
            return HandleResult(await Mediator.Send(command));
        }

    }
}
