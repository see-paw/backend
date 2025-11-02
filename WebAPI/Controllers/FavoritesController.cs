using Application.Core;
using Application.Favorites.Queries;
using Application.Favorites.Commands;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.DTOs.Animals;

namespace WebAPI.Controllers;

/// <summary>
/// API controller responsible for handling user favorite operations.
/// Provides endpoints for retrieving the authenticated user's favorite animals.
/// Handles operations related to managing user's favorite animals.
/// </summary>
/// <remarks>
/// Allows authenticated users to add or remove animals from their favorites list.  
/// Delegates business logic to the Application layer through MediatR.
/// </remarks>
public class FavoritesController(IMapper mapper) : BaseApiController
{
    /// <summary>
    /// Retrieves all favorite animals for the authenticated user.
    /// Adds an animal to the authenticated user's favorites list.
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
    /// <param name="animalId">The unique identifier of the animal to favorite.</param>
    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<ActionResult> GetUserFavorites(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetUserFavorites.Query
    [HttpPost("{animalId}")]
    public async Task<ActionResult<ResAnimalDto>> AddFavorite(string animalId)
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        var result = await Mediator.Send(new AddFavorite.Command { AnimalId = animalId });

        if (!result.IsSuccess || result.Value == null)
        {
        if (!result.IsSuccess)
            return HandleResult(result);

        var dto = mapper.Map<ResFavoriteAnimalDto>(result.Value);
        return HandleResult(Result<ResFavoriteAnimalDto>.Success(dto, 201));
        }

        var dtoList = mapper.Map<List<ResFavoriteAnimalDto>>(result.Value);
    /// <summary>
    /// Deactivates an existing animal from the authenticated user's favorites list.
    /// </summary>
    /// <param name="animalId">The unique identifier of the animal to remove from favorites.</param>
    [Authorize(Roles = "User")]
    [HttpPatch("{animalId}/deactivate")]
    public async Task<ActionResult<ResAnimalDto>> DeactivateFavorite(string animalId)
    {
        var result = await Mediator.Send(new DeactivateFavorite.Command { AnimalId = animalId });

        // Create a new paginated list with the DTOs
        var dtoPagedList = new PagedList<ResFavoriteAnimalDto>(
            dtoList,
            result.Value.TotalCount,
            result.Value.CurrentPage,
            result.Value.PageSize
        );
        if (!result.IsSuccess)
            return HandleResult(result);

        // Return the successful paginated result
        return HandleResult(Result<PagedList<ResFavoriteAnimalDto>>.Success(dtoPagedList, 200));
        var dto = mapper.Map<ResFavoriteAnimalDto>(result.Value);
        return HandleResult(Result<ResFavoriteAnimalDto>.Success(dto, 200));
    }
}