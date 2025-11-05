using Application.Auth.Commands;
using Application.Core;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs.Auth;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Handles user account-related operations such as registration and authentication.
    /// </summary>
    public class AccountController : BaseApiController
    {
        private readonly IMapper _mapper;

        public AccountController(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Registers a new user account. 
        /// Supports both standard users and AdminCAA accounts.
        /// </summary>
        /// <param name="reqRegisterUserDto">
        /// Registration request payload containing user information and desired role.
        /// </param>
        /// <returns>
        /// Returns a <see cref="ResRegisterUserDto"/> containing the created user information.
        /// If the role is <c>AdminCAA</c>, shelter information will also be included.
        /// </returns>
        /// <remarks>
        /// Accessible without authentication.
        /// </remarks>
        /// <response code="201">User successfully registered.</response>
        /// <response code="400">Validation error or registration failed.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] ReqRegisterUserDto reqRegisterUserDto)
        {
            // Map incoming DTO to the registration command
            var command = _mapper.Map<Register.Command>(reqRegisterUserDto);

            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
                return HandleResult(result);

            // Map the domain result to the response DTO
            var responseDto = _mapper.Map<ResRegisterUserDto>(result.Value);

            // Manually assign the role because it's not stored directly in the User entity
            responseDto.Role = reqRegisterUserDto.SelectedRole;

            // Only map shelter info when the registered user corresponds to an AdminCAA
            if (responseDto.Role == "AdminCAA" && result.Value.Shelter != null)
                responseDto.Shelter = _mapper.Map<ResRegisterShelterDto>(result.Value.Shelter);

            return HandleResult(Result<ResRegisterUserDto>.Success(responseDto, 201));
        }
    }
}
