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
