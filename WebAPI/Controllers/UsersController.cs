using Application.Core;
using Application.Interfaces;
using Application.Users.Commands;
using Application.Users.Queries;

using AutoMapper;

using Domain;

using Infrastructure;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebAPI.DTOs;
using WebAPI.DTOs.User;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Manages user profile operations, including retrieving and updating authenticated user data.
    /// </summary>
    public class UsersController(IUserAccessor userAccessor, IMapper mapper) : BaseApiController
    {
        /// <summary>
        /// Retrieves the profile information of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// A <see cref="ResUserProfileDto"/> object containing the authenticated user's data.
        /// </returns>
        [Authorize(Roles = "AdminCAA, User")]
        [HttpGet]
        public async Task<ActionResult<ResUserProfileDto>> GetUserProfile()
        {
            // Retrieve the authenticated user context
            var currentUser = await userAccessor.GetUserAsync();

            // Send the query to retrieve user profile data
            var result = await Mediator.Send(new GetUserProfile.Query
            {
                UserId = currentUser.Id
            });

            if (!result.IsSuccess)
                return HandleResult(result);

            // Map domain entity to DTO
            var userDto = mapper.Map<ResUserProfileDto>(result.Value);

            // Return standardized result
            return HandleResult(Result<ResUserProfileDto>.Success(userDto, 200));
        }

        /// <summary>
        /// Retrieves the role of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// A <see cref="ResUserRoleDto"/> containing the user's role ("User" or "AdminCAA").
        /// </returns>
        [Authorize(Roles = "AdminCAA, User")]
        [HttpGet("role")]
        public async Task<ActionResult<ResUserRoleDto>> GetUserRole()
        {
            var result = await Mediator.Send(new GetUserRole.Query());

            if (!result.IsSuccess)
                return HandleResult(Result<ResUserRoleDto>.Failure(result.Error!, result.Code));

            var roleDto = new ResUserRoleDto { Role = result.Value! };

            return HandleResult(Result<ResUserRoleDto>.Success(roleDto, 200));
        }

        /// <summary>
        /// Retrieves the ID of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// A <see cref="ResUserIdDto"/> containing the user's ID.
        /// </returns>
        [Authorize(Roles = "AdminCAA, User")]
        [HttpGet("id")]
        public async Task<ActionResult<ResUserIdDto>> GetUserId()
        {
            var result = await Mediator.Send(new GetUserId.Query());

            if (!result.IsSuccess)
                return HandleResult(Result<ResUserIdDto>.Failure(result.Error!, result.Code));

            var userIdDto = new ResUserIdDto { UserId = result.Value! };

            return HandleResult(Result<ResUserIdDto>.Success(userIdDto, 200));
        }

        /// <summary>
        /// Updates the profile information of the currently authenticated user.
        /// </summary>
        /// <param name="reqUserProfileDto">The request DTO containing updated user profile information.</param>
        /// <returns>
        /// A <see cref="ResUserProfileDto"/> containing the updated user data if the operation succeeds;
        /// otherwise, an appropriate error response.
        /// </returns>

        /// <summary>
        /// Updates the profile information of the currently authenticated user.
        /// </summary>
        /// <param name="reqUserProfileDto">The request DTO containing updated user profile information.</param>
        /// <returns>
        /// A <see cref="ResUserProfileDto"/> containing the updated user data if the operation succeeds;
        /// otherwise, an appropriate error response.
        /// </returns>
        [Authorize(Roles = "AdminCAA, User")]
        [HttpPut]
        public async Task<ActionResult<ResUserProfileDto>> EditUserProfile([FromBody] ReqUserProfileDto reqUserProfileDto)
        {
            // Retrieve the authenticated user context
            var currentUser = await userAccessor.GetUserAsync();

            if (currentUser == null)
                return HandleResult(Result<ResUserProfileDto>.Failure("User is not authenticated", 401));

            // Map the incoming DTO to a User domain entity
            var updatedUser = mapper.Map<User>(reqUserProfileDto);

            // Create and send the command 
            var command = new EditUserProfile.Command
            {
                UserId = currentUser.Id,
                UpdatedUser = updatedUser
            };

            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
                return HandleResult(result);

            // Map updated entity to response DTO
            var resDto = mapper.Map<ResUserProfileDto>(result.Value);

            // Return standardized result with HTTP 200
            return HandleResult(Result<ResUserProfileDto>.Success(resDto, 200));
        }
    }
}
