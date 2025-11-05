using Application.Auth.Commands;
using Application.Core;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs.Auth;

namespace WebAPI.Controllers;

public class AccountController : BaseApiController
{
    private readonly IMapper _mapper;

    public AccountController(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// Registers a new user or AdminCAA account.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] ReqRegisterUserDto reqRegisterUserDto)
    {
        var command = _mapper.Map<Register.Command>(reqRegisterUserDto);

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var responseDto = _mapper.Map<ResRegisterUserDto>(result.Value);
        responseDto.Role = reqRegisterUserDto.SelectedRole; // manual assignment because it's not mapped automatically since it's not part of User entity

        //only map Shelter info if the role is AdminCAA
        if (responseDto.Role == "AdminCAA" && result.Value.Shelter != null)
            responseDto.Shelter = _mapper.Map<ResRegisterShelterDto>(result.Value.Shelter);

        return HandleResult(Result<ResRegisterUserDto>.Success(responseDto, 201));
    }
}