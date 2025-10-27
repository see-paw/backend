using Application.Core;
using Application.Fosterings.Commands;
using Application.Fosterings.Queries;
using Application.Interfaces;
using AutoMapper;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;

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
        [Authorize(Roles = "User")]
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
        [Authorize(Roles = "User")]
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
    }
}


