using System;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Models;
using ResortManagement.Application.Features.Identity.Commands.LoginUser;
using ResortManagement.Application.Features.Identity.Commands.RegisterUser;
using ResortManagement.Application.Features.Identity.DTOs;

namespace ResortManagement.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<Guid>>> Register([FromBody] RegisterUserCommand command)
    {
        var userId = await _mediator.Send(command);
        return Ok(new ApiResponse<Guid>(userId, "User registered successfully."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new ApiResponse<AuthResponseDto>(result, "Login successful."));
    }
}
