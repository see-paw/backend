using Application.Auth.Commands;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Register([FromBody] ReqRegisterUserDto reqResiRegisterUserDto)
    {
        var user = _mapper.Map<User>(dreqResiRegisterUserDtoto);

        var result = await Mediator.Send(new Register.Command
        {
            User = user,
            Password = reqResiRegisterUserDto.Password,
            SelectedRole = reqResiRegisterUserDto.SelectedRole
        });

        return HandleResult(result);
    }
}