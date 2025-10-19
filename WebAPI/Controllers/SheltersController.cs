using Application.Core;
using Application.Shelters.Queries;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers;
using WebAPI.DTOs;

namespace API.Controllers
{
    /// <summary>
    /// API controller responsible for handling operations related to shelters.
    /// Provides endpoints for retrieving animals associated with a specific shelter.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SheltersController : BaseApiController
    {
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SheltersController"/> class.
        /// </summary>
        /// <param name="mapper">AutoMapper instance used for mapping between domain models and DTOs.</param>
        public SheltersController(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paginated list of animals that belong to a specified shelter.
        /// </summary>
        /// <param name="shelterId">The unique identifier of the shelter.</param>
        /// <param name="pageNumber">The page number to retrieve (default is 1).</param>
        /// <returns>
        /// A paginated <see cref="PagedList{T}"/> of <see cref="ResAnimalDto"/> objects representing the animals.
        /// Returns <c>400</c> if the page number is invalid or an appropriate error message if the shelter or animals are not found.
        /// </returns>
        [HttpGet("{shelterId}/animals")]
        public async Task<ActionResult<PagedList<Animal>>> GetAnimalsByShelter(string shelterId, [FromQuery] int pageNumber = 1)
        {
            if (pageNumber < 1)
                return BadRequest("Page number must be 1 or greater");

            const int pageSize = 20; // Default page size

            // Send the query to the Application layer through MediatR
            var result = await Mediator.Send(new GetAnimalsByShelter.Query
            {
                ShelterId = shelterId,
                PageNumber = pageNumber
            });

            // Return failure result if the handler reports an error
            if (!result.IsSuccess)
                return HandleResult(result);

            // Map Animal → ResAnimalDto for the API response
            var dtoList = _mapper.Map<List<ResAnimalDto>>(result.Value);

            // Wrap the DTO list in a paginated structure with metadata
            var pagedDtoList = new PagedList<ResAnimalDto>(
                dtoList,
                result.Value.TotalCount,
                result.Value.CurrentPage,
                result.Value.PageSize
            );

            // Return the standardized result using the base handler
            return HandleResult(Result<PagedList<ResAnimalDto>>.Success(pagedDtoList));
        }
    }
}
