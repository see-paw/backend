using Application.Common;
using Application.Core;
using Application.Interfaces;
using Application.OwnershipRequests.Queries;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.DTOs.Ownership;

namespace WebAPI.Controllers;

/// <summary>
/// Provides endpoints for retrieving ownership-related information for the authenticated user.
/// </summary>
/// <remarks>
/// This controller exposes two endpoints:
/// <list type="bullet">
///   <item><description><c>GET /api/ownerships/requests</c> – Retrieves all adoption requests made by the user.</description></item>
///   <item><description><c>GET /api/ownerships/owned-animals</c> – Retrieves animals currently owned by the user.</description></item>
/// </list>
/// Access is restricted to users with the <c>User</c> role.
/// </remarks>
[Authorize(Roles = AppRoles.User)]
public class OwnershipsController(IMapper mapper) : BaseApiController
{
    /// <summary>
    /// Retrieves all ownership (adoption) requests submitted by the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Returns both active and recently rejected requests (within one month of rejection).
    /// Related entities such as the <see cref="Domain.Animal"/>, <see cref="Domain.Breed"/>, and <see cref="Domain.Shelter"/>
    /// are automatically included in the query for richer response data.
    /// </remarks>
    /// <returns>
    /// A <see cref="List{ResUserOwnershipsDto}"/> containing the user’s ownership requests, or an appropriate HTTP error code:
    /// <list type="bullet">
    ///   <item><description><b>200 OK</b> – Requests retrieved successfully.</description></item>
    ///   <item><description><b>401 Unauthorized</b> – The user is not authenticated or lacks the required role.</description></item>
    ///   <item><description><b>404 Not Found</b> – The user does not exist in the database.</description></item>
    /// </list>
    /// </returns>
    [HttpGet("requests")]
    public async Task<ActionResult> GetUserOwnerships()
    {
        var result = await Mediator.Send(new GetOwnershipRequestsByUser.Query());
        
        if (!result.IsSuccess)
            return HandleResult(result);

        var dtoRequests = mapper.Map<List<ResUserOwnershipsDto>>(result.Value);
        return HandleResult(Result<List<ResUserOwnershipsDto>>.Success(dtoRequests, 200));

    }
    
    /// <summary>
    /// Retrieves all animals currently owned by the authenticated user.
    /// </summary>
    /// <remarks>
    /// Returns a list of animals linked to the user through the <c>OwnerId</c> field.
    /// The data includes detailed animal information (breed, shelter, and associated images).
    /// </remarks>
    /// <returns>
    /// A <see cref="List{ResUserOwnershipsDto}"/> containing the user’s owned animals, or an appropriate HTTP error code:
    /// <list type="bullet">
    ///   <item><description><b>200 OK</b> – Owned animals retrieved successfully.</description></item>
    ///   <item><description><b>401 Unauthorized</b> – The user is not authenticated or lacks permissions.</description></item>
    ///   <item><description><b>404 Not Found</b> – The user does not exist in the database.</description></item>
    /// </list>
    /// </returns>
    [HttpGet("owned-animals")]
    public async Task<IActionResult> GetOwnedAnimals()
    {
        var result = await Mediator.Send(new GetUserOwnedAnimals.Query());
        
        if (!result.IsSuccess)
            return HandleResult(result);

        var dtoOwned = mapper.Map<List<ResUserOwnershipsDto>>(result.Value);
        return HandleResult(Result<List<ResUserOwnershipsDto>>.Success(dtoOwned, 200));
    }
    
}