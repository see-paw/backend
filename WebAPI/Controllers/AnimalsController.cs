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
        /// A paginated <see cref="PagedList{T}"/> of <see cref="ResAnimalDto"/> objects representing the animals.
        /// Returns <c>400</c> if the page number is invalid or an appropriate error message on failure.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<PagedList<ResAnimalDto>>> GetAnimals([FromQuery] int pageNumber = 1)
        {
            if (pageNumber < 1)
                return BadRequest("Page number must be 1 or greater.");

            var result = await Mediator.Send(new GetAnimalList.Query
            {
                PageNumber = pageNumber
            });

            if (!result.IsSuccess)
                return HandleResult(result);

            // Map the list of Animal entities to response DTOs
            var dtoList = _mapper.Map<List<ResAnimalDto>>(result.Value);

            // Create a new paginated list with the DTOs
            var dtoPagedList = new PagedList<ResAnimalDto>(
                dtoList,
                result.Value.TotalCount,
                result.Value.CurrentPage,
                result.Value.PageSize
            );

            // Return the successful paginated result
            return HandleResult(Result<PagedList<ResAnimalDto>>.Success(dtoPagedList));
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
        public async Task<ActionResult<string>> CreateAnimal([FromBody] ReqCreateAnimalDto reqAnimalDto)
        {
            // Temporary shelter ID (to be replaced when JWT authentication is implemented)
            var shelterId = "22222222-2222-2222-2222-222222222222";

            if (string.IsNullOrEmpty(shelterId))
                return Unauthorized("Invalid shelter token");

            // Map the validated DTO to the domain entity
            var animal = _mapper.Map<Animal>(reqAnimalDto);

            List<Image>? images = null;
            if (reqAnimalDto.Images != null && reqAnimalDto.Images.Any())
            {
                images = _mapper.Map<List<Image>>(reqAnimalDto.Images);
            }

            // Build the command to send to the Application layer
            var command = new CreateAnimal.Command
            {
                Animal = animal,
                ShelterId = shelterId,
                Images = images
            };

            // Centralized result handling (200, 400, 404, etc.)
            return HandleResult(await Mediator.Send(command));
        }
    }
}