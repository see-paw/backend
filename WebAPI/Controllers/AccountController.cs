using Application.Auth.Commands;
using Application.Core;
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
    public async Task<IActionResult> Register([FromBody] ReqRegisterUserDto reqRegisterUserDto)
    {
        var user = _mapper.Map<User>(reqRegisterUserDto);

        var result = await Mediator.Send(new Register.Command
        {
            //the User entity is mapped from the ReqRegisterUserDto but the other fields
            //are set manually because they are not part of User entity
            User = user,
            Password = reqRegisterUserDto.Password,
            SelectedRole = reqRegisterUserDto.SelectedRole,
            ShelterName = reqRegisterUserDto.ShelterName,
            ShelterStreet = reqRegisterUserDto.ShelterStreet,
            ShelterCity = reqRegisterUserDto.ShelterCity,
            ShelterPostalCode = reqRegisterUserDto.ShelterPostalCode,
            ShelterPhone = reqRegisterUserDto.ShelterPhone,
            ShelterNIF = reqRegisterUserDto.ShelterNIF,
            ShelterOpeningTime = reqRegisterUserDto.ShelterOpeningTime,
            ShelterClosingTime = reqRegisterUserDto.ShelterClosingTime
        });

        if (!result.IsSuccess)
            return HandleResult(result);

        var responseDto = _mapper.Map<ResRegisterUserDto>(result.Value);
        responseDto.Role = reqRegisterUserDto.SelectedRole; // manual assignment because it's not mapped automatically since it's not part of User entity

        return HandleResult(Result<ResRegisterUserDto>.Success(responseDto, 201));
    }
}