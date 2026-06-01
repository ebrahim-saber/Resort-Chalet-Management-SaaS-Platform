using System;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Models;
using ResortManagement.Application.Features.Resorts.Commands.CreateResort;
using ResortManagement.Application.Features.Resorts.Commands.CreateUnitType;

namespace ResortManagement.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/resorts")]
[ApiController]
[Authorize]
public class ResortsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ResortsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateResortCommand command)
    {
        var resortId = await _mediator.Send(command);
        return Ok(new ApiResponse<Guid>(resortId, "Resort created successfully."));
    }

    [HttpPost("unit-types")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateUnitType([FromBody] CreateUnitTypeCommand command)
    {
        var unitTypeId = await _mediator.Send(command);
        return Ok(new ApiResponse<Guid>(unitTypeId, "Unit Type registered successfully."));
    }
}
