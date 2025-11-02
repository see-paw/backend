using Application.Core;
using Application.Favorites.Commands;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.DTOs.Animals;

namespace WebAPI.Controllers;

/// <summary>
/// Handles operations related to managing user's favorite animals.
/// </summary>
/// <remarks>
/// Allows authenticated users to add or remove animals from their favorites list.  
/// Delegates business logic to the Application layer through MediatR.
/// </remarks>
public class FavoritesController(IMapper mapper) : BaseApiController
{
    /// <summary>
    /// Adds an animal to the authenticated user's favorites list.
    /// </summary>
    /// <param name="animalId">The unique identifier of the animal to favorite.</param>
    [Authorize(Roles = "User")]
    [HttpPost("{animalId}")]
    public async Task<ActionResult<ResAnimalDto>> AddFavorite(string animalId)
    {
        var result = await Mediator.Send(new AddFavorite.Command { AnimalId = animalId });

        if (!result.IsSuccess)
            return HandleResult(result);

        var dto = mapper.Map<ResFavoriteAnimalDto>(result.Value);
        return HandleResult(Result<ResFavoriteAnimalDto>.Success(dto, 201));
    }

    /// <summary>
    /// Deactivates an existing animal from the authenticated user's favorites list.
    /// </summary>
    /// <param name="animalId">The unique identifier of the animal to remove from favorites.</param>
    [Authorize(Roles = "User")]
    [HttpPatch("{animalId}/deactivate")]
    public async Task<ActionResult<ResAnimalDto>> DeactivateFavorite(string animalId)
    {
        var result = await Mediator.Send(new DeactivateFavorite.Command { AnimalId = animalId });

        if (!result.IsSuccess)
            return HandleResult(result);

        var dto = mapper.Map<ResFavoriteAnimalDto>(result.Value);
        return HandleResult(Result<ResFavoriteAnimalDto>.Success(dto, 200));
    }
}
