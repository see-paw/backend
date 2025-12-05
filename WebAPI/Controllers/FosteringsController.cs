using Application.Core;
using Application.Fosterings.Commands;
using Application.Fosterings.Queries;
using Application.Interfaces;

using AutoMapper;

using Domain.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebAPI.DTOs.Fostering;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Provides endpoints for managing fostering relationships between users and animals.
    /// </summary>
    public class FosteringsController(IMapper mapper, IUserAccessor userAccessor) : BaseApiController
    {
        /// <summary>
        /// Retrieves the list of active fosterings for the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// This endpoint returns only fosterings with <c>Status = Active</c> that belong to the authenticated user.
        /// Each entry includes the animal’s name, age, images, monthly contribution amount, and fostering start date.
        /// </remarks>
        /// <returns>
        /// A list of <see cref="ResActiveFosteringDto"/> objects representing the active fosterings of the user.
        /// </returns>
        /// <response code="200">Returns a list of active fosterings for the user.</response>
        /// <response code="401">Returned if the user is not authenticated or lacks a valid token.</response>
        [Authorize(Roles = AppRoles.User)]
        [HttpGet]
        public async Task<ActionResult<List<ResActiveFosteringDto>>> GetActiveFosterings()
        {
            var user = await userAccessor.GetUserAsync();
            if (user is null)
                return HandleResult(Result<List<ResActiveFosteringDto>>.Failure("User not authenticated.", 401));

            var result = await Mediator.Send(new GetActiveFosterings.Query { UserId = user.Id });
            if (!result.IsSuccess) return HandleResult(result);

            var dtoList = mapper.Map<List<ResActiveFosteringDto>>(result.Value);
            return HandleResult(Result<List<ResActiveFosteringDto>>.Success(dtoList, 200));
        }


        /// <summary>
        /// Cancela um apadrinhamento ativo do utilizador autenticado.
        /// </summary>
        /// <param name="id">O identificador único do apadrinhamento a cancelar.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        [Authorize(Roles = AppRoles.User)]
        [HttpPatch("{id}/cancel")]
        public async Task<ActionResult> CancelFostering(string id)
        {
            var userId = userAccessor.GetUserId();

            var command = new CancelFostering.Command
            {
                FosteringId = id,
                UserId = userId
            };

            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
                return HandleResult(result);

            var dto = mapper.Map<ResCancelFosteringDto>(result.Value);
            return HandleResult(Result<ResCancelFosteringDto>.Success(dto, 200));
        }


        /// <summary>
        /// Creates a new fostering record for the specified animal, allowing the authenticated user
        /// to sponsor it with a monthly contribution.
        /// </summary>
        /// <param name="animalId">The unique identifier (GUID) of the animal to be fostered.</param>
        /// <param name="reqAddFosteringDto">The data transfer object containing the monthly contribution value.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a <see cref="ResActiveFosteringDto"/> that represents the created fostering record.
        /// Returns an appropriate error response if the operation fails.
        /// <list type="bullet">
        /// <item><description><c>201 Created</c> – fostering created successfully.</description></item>
        /// <item><description><c>404 Not Found</c> – the specified animal does not exist.</description></item>
        /// <item><description><c>409 Conflict</c> – the animal is in an invalid state or already fostered by the same user.</description></item>
        /// <item><description><c>422 Unprocessable Entity</c> – the monthly value surpasses the animal’s cost.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This endpoint is accessible only to authenticated users with the <c>User</c> role.
        /// </remarks>
        [Authorize(Roles = "User")]
        [HttpPost("{animalId}/fosterings")]
        public async Task<ActionResult<ResActiveFosteringDto>> AddFostering(string animalId, [FromBody] ReqAddFosteringDto reqAddFosteringDto)
        {
            var result = await Mediator.Send(new AddFostering.Command
            {
                AnimalId = animalId,
                MonthValue = reqAddFosteringDto.MonthValue

            });

            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves the IDs of active fosterings for the authenticated user.
        /// </summary>
        /// <returns>List of fostering IDs and their associated animal IDs.</returns>
        /// <response code="200">Returns the list of IDs.</response>
        /// <response code="404">User not found.</response>
        [Authorize(Roles = AppRoles.User)]
        [HttpGet("ids")]
        public async Task<ActionResult<List<ResActiveFosteringIdDto>>> GetActiveFosteringIds()
        {
            var result = await Mediator.Send(new GetActiveFosteringIds.Query());
            if (!result.IsSuccess) return HandleResult(result);

            var dtoList = result.Value!.Select(f => new ResActiveFosteringIdDto
            {
                Id = f.Id,
                AnimalId = f.AnimalId
            }).ToList();

            return HandleResult(Result<List<ResActiveFosteringIdDto>>.Success(dtoList, 200));
        }
    }
}


