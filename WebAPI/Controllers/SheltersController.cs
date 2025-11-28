using Application.Animals.Filters;
using Application.Core;
using Application.Shelters.Queries;

using AutoMapper;

using Domain;
using Domain.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebAPI.DTOs.Animals;
using WebAPI.DTOs.Shelter;

namespace WebAPI.Controllers
{
    /// <summary>
    /// API controller responsible for handling operations related to shelters.
    /// Provides endpoints for retrieving animals associated with a specific shelter.
    /// </summary>
    public class SheltersController : BaseApiController
    {
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SheltersController"/> class.
        /// </summary>
        /// <param name="mapper">AutoMapper instance used to map domain entities to DTOs.</param>
        public SheltersController(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paginated, optionally filtered and sorted list of animals
        /// that belong to a specified shelter.
        /// </summary>
        /// <param name="shelterId">The unique identifier of the shelter whose animals are to be retrieved.</param>
        /// <param name="pageNumber">
        /// The page number to retrieve for pagination. Defaults to <c>1</c>.
        /// Must be a positive integer.
        /// </param>
        /// <param name="filters">
        /// Optional filter criteria applied to the animal list.
        /// Supports fields such as species, size, sex, breed, age range and name.
        /// </param>
        /// <param name="sortBy">
        /// Optional field name to sort the results by.
        /// Examples: <c>name</c>, <c>age</c>, <c>createdAt</c>.
        /// </param>
        /// <param name="order">
        /// Sorting direction. Accepted values: <c>asc</c> or <c>desc</c>.
        /// Defaults to backend-defined ordering if not provided.
        /// </param>
        /// <returns>
        /// Returns a paginated <see cref="PagedList{T}"/> containing <see cref="ResAnimalDto"/> items.
        /// Possible responses:
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> — Paginated list of animals successfully retrieved.</description></item>
        /// <item><description><c>400 Bad Request</c> — Invalid page number or invalid filters.</description></item>
        /// <item><description><c>404 Not Found</c> — Shelter not found or shelter contains no animals.</description></item>
        /// <item><description><c>403 Forbidden</c> — User does not have Admin CAA permissions.</description></item>
        /// </list>
        /// </returns>
        [Authorize(Roles = AppRoles.AdminCAA)]
        [HttpGet("{shelterId}/animals")]
        public async Task<ActionResult<PagedList<Animal>>> GetAnimalsByShelter(string shelterId, [FromQuery] int pageNumber = 1, [FromQuery] AnimalFilterModel? filters = null, [FromQuery] string? sortBy = null, [FromQuery] string? order = null)
        {

            // Send the query to the Application layer
            var result = await Mediator.Send(new GetAnimalsByShelter.Query
            {
                ShelterId = shelterId,
                PageNumber = pageNumber,
                Filters = filters,
                SortBy = sortBy,
                Order = order
            });

            // Return failure result if the handler reports an error
            if (!result.IsSuccess)
                return HandleResult(result);

            // Map Animal to ResAnimalDto for the API response
            var dtoList = _mapper.Map<List<ResAnimalDto>>(result.Value.Items);

            // Wrap the DTO list in a paginated structure with metadata
            var pagedDtoList = new PagedList<ResAnimalDto>(
                dtoList,
                result.Value.TotalCount,
                result.Value.CurrentPage,
                result.Value.PageSize
            );

            // Return the standardized result using the base handler
            return HandleResult(Result<PagedList<ResAnimalDto>>.Success(pagedDtoList, 200));
        }

        /// <summary>
        /// Retrieves detailed information about a specific shelter.
        /// </summary>
        /// <param name="shelterId">The unique identifier of the shelter.</param>
        /// <returns>
        /// A <see cref="ResShelterInfoDto"/> with shelter information, or an appropriate error response.
        /// </returns>
        [Authorize(Roles = "User,AdminCAA")]
        [HttpGet("{shelterId}")]
        public async Task<ActionResult<ResShelterInfoDto>> GetShelterInfo(string shelterId)
        {
            // 1️⃣ Chamar a Query na Application
            var result = await Mediator.Send(new GetShelterInfo.Query(shelterId));

            // 2️⃣ Se falhar, delegar para o HandleResult com o Result<Shelter>
            if (!result.IsSuccess)
                return HandleResult(result);

            // 3️⃣ Mapear Domain.Shelter -> ResShelterInfoDto
            var dto = _mapper.Map<ResShelterInfoDto>(result.Value);

            // 4️⃣ Reembrulhar em Result<ResShelterInfoDto> e usar HandleResult
            return HandleResult(Result<ResShelterInfoDto>.Success(dto, 200));
        }

    }
}
