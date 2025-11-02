using Application.Core;
using Application.Favorites.Queries;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;

namespace WebAPI.Controllers;

/// <summary>
/// API controller responsible for handling user favorite operations.
/// Provides endpoints for retrieving the authenticated user's favorite animals.
/// </summary>
public class FavoritesController(IMapper mapper) : BaseApiController
{
    /// <summary>
    /// Retrieves all favorite animals for the authenticated user.
    /// </summary>
    /// <param name="pageNumber">
    /// The current page number for pagination. Defaults to <c>1</c>.
    /// </param>
    /// <param name="pageSize">
    /// The number of items per page. Defaults to <c>20</c>.
    /// </param>
    /// <returns>
    /// A paginated <see cref="PagedList{T}"/> of <see cref="ResFavoriteAnimalDto"/> objects 
    /// representing animals marked as favorites by the user.
    /// </returns>
    /// <remarks>
    /// Only active favorites are returned. The user ID is extracted from the JWT token.
    /// Results include complete animal information including images, breed, and shelter details.
    /// </remarks>
    /// <response code="200">Returns a paginated list of favorite animals.</response>
    /// <response code="401">Unauthorized — if the user is not authenticated.</response>
    /// <response code="500">Internal server error — if an unexpected error occurs.</response>
    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<ActionResult> GetUserFavorites(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetUserFavorites.Query
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        });

        if (!result.IsSuccess || result.Value == null)
        {
            return HandleResult(result);
        }

        var dtoList = mapper.Map<List<ResFavoriteAnimalDto>>(result.Value);

        // Create a new paginated list with the DTOs
        var dtoPagedList = new PagedList<ResFavoriteAnimalDto>(
            dtoList,
            result.Value.TotalCount,
            result.Value.CurrentPage,
            result.Value.PageSize
        );

        // Return the successful paginated result
        return HandleResult(Result<PagedList<ResFavoriteAnimalDto>>.Success(dtoPagedList, 200));
    }
}